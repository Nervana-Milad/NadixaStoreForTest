using ClosedXML.Excel;
using Nadixa.Application.DTOS.Product;
using Nadixa.Application.DTOS.ProductCategory;
using Nadixa.Application.DTOS.ProductSubCategory;
using Nadixa.Application.Interfaces;
using Nadixa.Application.Interfaces.Excel;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class ProductImportExportService : IProductImportExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExcelService _excelService;
        private readonly ICategoryService _categoryService;
        private readonly ISubCategoryService _subCategoryService;
        private readonly StockNotificationService _stockNotificationService;

        public ProductImportExportService(
            IUnitOfWork unitOfWork,
            IExcelService excelService,
            ICategoryService categoryService,
            ISubCategoryService subCategoryService,
            StockNotificationService stockNotificationService)
        {
            _unitOfWork = unitOfWork;
            _excelService = excelService;
            _categoryService = categoryService;
            _subCategoryService = subCategoryService;
            _stockNotificationService = stockNotificationService;
        }



        public async Task<Stream> ExportToExcelAsync(CancellationToken ct = default)
        {
            return await _excelService.ExportAsync(GetProductsForExportAsync(), "Products", ct);
        }


        private async IAsyncEnumerable<ProductExportDto> GetProductsForExportAsync()
        {
            var products = await _unitOfWork.Repository<Product>().GetAllAsync(
                p => p.ProductCategory,
                p => p.ProductSubCategory);

            foreach (var product in products.OrderBy(p => p.Id))
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
                await Task.Yield();
            }
        }


        public async Task<ProductImportResultDto> ImportFromExcelAsync(Stream fileStream, CancellationToken ct = default)
        {
            var result = new ProductImportResultDto();
            var productsToNotify = new List<int>();

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // skip header

            // نجيب الكاتيجوريز والصب-كاتيجوريز مرة واحدة، ونحدّثهم في الميموري كل ما نضيف واحدة جديدة
            var categories = (await _categoryService.GetAllAsync()).ToList();
            var subCategories = (await _subCategoryService.GetAllAsync()).ToList();

            foreach (var row in rows)
            {
                await ProcessRowAsync(row, categories, subCategories, result, productsToNotify);
            }

            await _unitOfWork.CompleteAsync();

            if (productsToNotify.Any())
                await _stockNotificationService.NotifySubscribersForMultipleAsync(productsToNotify);

            return result;
        }



        private async Task ProcessRowAsync(
            IXLRow row,
            List<CategoryDto> categories,
            List<SubCategoryDto> subCategories,
            ProductImportResultDto result,
            List<int> productsToNotify)
        {
            try
            {
                int id = 0;
                var idCell = row.Cell(1);
                if (!idCell.IsEmpty())
                    int.TryParse(idCell.GetValue<string>(), out id);

                string name = row.Cell(2).GetValue<string>();
                string description = row.Cell(3).GetValue<string>();
                decimal price = row.Cell(4).GetValue<decimal>();
                decimal? oldPrice = row.Cell(5).GetValue<decimal>();
                int stockQuantity = row.Cell(6).GetValue<int>();
                string categoryName = row.Cell(7).GetValue<string>();
                string subCategoryName = row.Cell(8).GetValue<string>();

                if (string.IsNullOrWhiteSpace(name))
                {
                    result.Failed++;
                    result.Errors.Add($"Row {row.RowNumber()}: Name is required.");
                    return;
                }

                // ===== Category =====
                var category = categories.FirstOrDefault(c => c.Name == categoryName);
                if (category == null)
                {
                    await _categoryService.CreateAsync(new CategoryCreateDto { Name = categoryName });
                    categories.Clear();
                    categories.AddRange(await _categoryService.GetAllAsync());
                    category = categories.FirstOrDefault(c => c.Name == categoryName);
                    result.NewCategories++;
                }

                if (category == null)
                {
                    result.Failed++;
                    result.Errors.Add($"Row {row.RowNumber()}: Failed to resolve category '{categoryName}'.");
                    return;
                }

                // ===== SubCategory =====
                var subCategory = subCategories.FirstOrDefault(
                    s => s.Name == subCategoryName && s.ProductCategoryId == category.Id);

                if (subCategory == null)
                {
                    await _subCategoryService.CreateAsync(new SubCategoryCreateDto
                    {
                        Name = subCategoryName,
                        ProductCategoryId = category.Id
                    });
                    subCategories.Clear();
                    subCategories.AddRange(await _subCategoryService.GetAllAsync());
                    subCategory = subCategories.FirstOrDefault(
                        s => s.Name == subCategoryName && s.ProductCategoryId == category.Id);
                    result.NewSubCategories++;
                }

                if (subCategory == null)
                {
                    result.Failed++;
                    result.Errors.Add($"Row {row.RowNumber()}: Failed to resolve sub-category '{subCategoryName}'.");
                    return;
                }

                await CreateOrUpdateProductAsync(id, name, description, price, oldPrice, stockQuantity,
                    category.Id, subCategory.Id, row.RowNumber(), result, productsToNotify);
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Errors.Add($"Row {row.RowNumber()}: {ex.Message}");
            }
        }


        private async Task CreateOrUpdateProductAsync(
            int id,
            string name,
            string description,
            decimal price,
            decimal? oldPrice,
            int stockQuantity,
            int categoryId,
            int subCategoryId,
            int rowNumber,
            ProductImportResultDto result,
            List<int> productsToNotify)
        {
            if (id > 0)
            {
                // ===== Update =====
                var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
                if (product == null)
                {
                    result.Failed++;
                    result.Errors.Add($"Row {rowNumber}: Product with Id {id} not found.");
                    return;
                }

                bool hasChanges =
                    product.Name != name ||
                    product.Description != description ||
                    product.Price != price ||
                    product.OldPrice != (oldPrice > 0 ? oldPrice : null) ||
                    product.StockQuantity != stockQuantity ||
                    product.ProductCategoryId != categoryId ||
                    product.ProductSubCategoryId != subCategoryId;

                if (!hasChanges) return;

                int oldStock = product.StockQuantity;

                product.Name = name;
                product.Description = description;
                product.Price = price;
                product.OldPrice = oldPrice > 0 ? oldPrice : null;
                product.StockQuantity = stockQuantity;
                product.ProductCategoryId = categoryId;
                product.ProductSubCategoryId = subCategoryId;

                _unitOfWork.Repository<Product>().Update(product);

                if (oldStock <= 0 && stockQuantity > 0)
                    productsToNotify.Add(product.Id);

                result.Updated++;
            }
            else
            {
                // ===== Create =====
                var newProduct = new Product
                {
                    Name = name,
                    Description = description,
                    Price = price,
                    OldPrice = oldPrice > 0 ? oldPrice : null,
                    StockQuantity = stockQuantity,
                    ProductCategoryId = categoryId,
                    ProductSubCategoryId = subCategoryId
                };

                await _unitOfWork.Repository<Product>().AddAsync(newProduct);
                result.Created++;
            }
        }

    }
}
