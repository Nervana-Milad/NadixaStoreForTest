using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces.Excel;
using Nadixa.Core.DTOS;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Infrastructure.Services;

namespace Nadixa.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly IExcelService _excelService;
        private readonly StockNotificationService _stockNotificationService;

        public AdminProductController(NadixaDbContext context, IExcelService excelService, StockNotificationService stockNotificationService)
        {
            _context = context;
            _excelService = excelService;
            _stockNotificationService = stockNotificationService; // 👈 ضيفي دي

        }

        public async Task<IActionResult> ExportToExcel(CancellationToken ct = default)
        {
            var stream = await _excelService.ExportAsync(GetProductsAsync(), "Products", ct);

            return File(stream,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Products_{DateTime.Now:yyyyMMdd}.xlsx");
        }


        private async IAsyncEnumerable<ProductExportDto> GetProductsAsync()
        {
            await foreach (var product in _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductSubCategory)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Id)
                .AsAsyncEnumerable())
            {
                yield return new ProductExportDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    OldPrice = product.OldPrice ?? 0,
                    StockQuantity = product.StockQuantity,
                    CategoryName = product.ProductCategory?.Name ?? "",
                    SubCategoryName = product.ProductSubCategory?.Name ?? ""
                };
            }
        }


        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please upload a valid Excel file.";
                return RedirectToAction("Index", "AdminDashboard");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xlsx")
            {
                TempData["Error"] = "Only .xlsx files are allowed.";
                return RedirectToAction("Index", "AdminDashboard");
            }

            int created = 0;
            int updated = 0;
            int failed = 0;
            int newCategories = 0;
            int newSubCategories = 0;
            var errors = new List<string>();
            var productsToNotify = new List<int>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header

            foreach (var row in rows)
            {
                try
                {
                    int id = 0;
                    var idCell = row.Cell(1);
                    if (!idCell.IsEmpty())
                    {
                        var cellValue = idCell.GetValue<string>();
                        int.TryParse(cellValue, out id);
                    }
                    Console.WriteLine($"Row {row.RowNumber()}: id = '{id}'");

                    string name = row.Cell(2).GetValue<string>();
                    string description = row.Cell(3).GetValue<string>();
                    decimal price = row.Cell(4).GetValue<decimal>();
                    decimal? oldPrice = row.Cell(5).GetValue<decimal>();
                    int stockQuantity = row.Cell(6).GetValue<int>();
                    string categoryName = row.Cell(7).GetValue<string>();
                    string subCategoryName = row.Cell(8).GetValue<string>();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        failed++;
                        errors.Add($"Row {row.RowNumber()}: Name is required.");
                        continue;
                    }

                    // هات الـ Category
                    var category = await _context.ProductCategories
                        .FirstOrDefaultAsync(c => c.Name == categoryName);

                    if (category == null)
                    {
                        category = new ProductCategory { Name = categoryName };
                        _context.ProductCategories.Add(category);
                        await _context.SaveChangesAsync();
                        newCategories++;
                    }

                    // هات الـ SubCategory
                    var subCategory = await _context.ProductSubCategories
                        .FirstOrDefaultAsync(s => s.Name == subCategoryName
                            && s.ProductCategoryId == category.Id);

                    if (subCategory == null)
                    {
                        subCategory = new ProductSubCategory
                        {
                            Name = subCategoryName,
                            ProductCategoryId = category.Id
                        };
                        _context.ProductSubCategories.Add(subCategory);
                        await _context.SaveChangesAsync();
                        newSubCategories++;
                    }
                    if (id > 0)
                    {
                        // Update
                        var product = await _context.Products
                            .FirstOrDefaultAsync(p => p.Id == id);

                        if (product == null)
                        {
                            failed++;
                            errors.Add($"Row {row.RowNumber()}: Product with Id {id} not found.");
                            continue;
                        }
                        bool hasChanges =
                           product.Name != name ||
                           product.Description != description ||
                           product.Price != price ||
                           product.OldPrice != (oldPrice > 0 ? oldPrice : null) ||
                           product.StockQuantity != stockQuantity ||
                           product.ProductCategoryId != category.Id ||
                           product.ProductSubCategoryId != subCategory.Id;

                        if (!hasChanges)
                        {
                            continue; // مفيش تغيير، سيبيه من غير ما نزود العداد أو نعمل Update
                        }

                        int oldStock = product.StockQuantity;

                        product.Name = name;
                        product.Description = description;
                        product.Price = price;
                        product.OldPrice = oldPrice > 0 ? oldPrice : null;
                        product.StockQuantity = stockQuantity;
                        product.ProductCategoryId = category.Id;
                        product.ProductSubCategoryId = subCategory.Id;
                        product.UpdatedAt = DateTime.Now;
                        if (oldStock <= 0 && stockQuantity > 0)   // 👈 3) ضيفي الشرط ده بعد التعديل
                        {
                            productsToNotify.Add(product.Id);
                        }
                        updated++;
                    }
                    else
                    {
                        // Create
                        var newProduct = new Product
                        {
                            Name = name,
                            Description = description,
                            Price = price,
                            OldPrice = oldPrice > 0 ? oldPrice : null,
                            StockQuantity = stockQuantity,
                            ProductCategoryId = category.Id,
                            ProductSubCategoryId = subCategory.Id,
                            CreatedAt = DateTime.Now
                        };

                        await _context.Products.AddAsync(newProduct);
                        created++;
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    errors.Add($"Row {row.RowNumber()}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();

            if (productsToNotify.Any())   // 👈 4) ضيفي السطرين دول بعد الـ SaveChangesAsync مباشرة
            {
                await _stockNotificationService.NotifySubscribersForMultipleAsync(productsToNotify);
            }

            TempData["Success"] = $"Import completed: {created} created, {updated} updated, {failed} failed.";


            if (newCategories > 0 || newSubCategories > 0)
            {
                TempData["Warning"] = $"{newCategories} new category(s) and {newSubCategories} new subcategory(s) were created without images. Please add images from the Categories page.";
            }

            if (errors.Any())
                TempData["Error"] = string.Join(" | ", errors);

            return RedirectToAction("Index", "AdminDashboard");
        }
    }
}
