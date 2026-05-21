using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class ProductSubCategoryController : Controller
    {
        private readonly NadixaDbContext _context;

        public ProductSubCategoryController(NadixaDbContext context)
        {
            _context = context;
        }
        // INDEX
        public async Task<IActionResult> Index()
        {
            var subCategories = await _context.ProductSubCategories
                .Include(s => s.ProductCategory)
                .ToListAsync();

            return View(subCategories);
        }

        // CREATE GET
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductSubCategoryViewModel
            {
                Categories = await _context.ProductCategories
                    .Select(c => new ProductCategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToListAsync()
            };

            return View(vm);
        }

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductSubCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.ProductCategories
                    .Select(c => new ProductCategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToListAsync();

                return View(model);
            }

            string? imagePath = null;

            // IMAGE
            if (model.ImageFile != null)
            {
                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/subcategories");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName = Guid.NewGuid().ToString()
                    + Path.GetExtension(model.ImageFile.FileName);

                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imagePath = "/images/subcategories/" + fileName;
            }

            var subCategory = new ProductSubCategory
            {
                Name = model.Name,
                Description = model.Description,
                ImageUrl = imagePath,
                ProductCategoryId = model.ProductCategoryId
            };

            _context.ProductSubCategories.Add(subCategory);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // EDIT GET
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var subCategory = await _context.ProductSubCategories
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subCategory == null)
            {
                return NotFound();
            }

            var vm = new ProductSubCategoryViewModel
            {
                Id = subCategory.Id,
                Name = subCategory.Name,
                Description = subCategory.Description,
                ImageUrl = subCategory.ImageUrl,
                ProductCategoryId = subCategory.ProductCategoryId,

                Categories = await _context.ProductCategories
                    .Select(c => new ProductCategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToListAsync()
            };

            return View(vm);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(ProductSubCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.ProductCategories
                    .Select(c => new ProductCategoryViewModel
                    {
                        Id = c.Id,
                        Name = c.Name
                    }).ToListAsync();

                return View(model);
            }

            var subCategory = await _context.ProductSubCategories
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (subCategory == null)
            {
                return NotFound();
            }

            // UPDATE IMAGE
            if (model.ImageFile != null)
            {
                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/subcategories");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName = Guid.NewGuid().ToString()
                    + Path.GetExtension(model.ImageFile.FileName);

                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                subCategory.ImageUrl =
                    "/images/subcategories/" + fileName;
            }

            // UPDATE DATA
            subCategory.Name = model.Name;
            subCategory.Description = model.Description;
            subCategory.ProductCategoryId = model.ProductCategoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var subCategory = await _context.ProductSubCategories
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subCategory == null)
            {
                return NotFound();
            }
            var hasProducts = await _context.Products
    .AnyAsync(p => p.ProductSubCategoryId == id);

            if (hasProducts)
            {
                TempData["Error"] =
                    "Cannot delete subcategory because it contains products.";

                return RedirectToAction(nameof(Index));
            }

            _context.ProductSubCategories.Remove(subCategory);

            await _context.SaveChangesAsync();

            TempData["Success"] = "SubCategory deleted successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
