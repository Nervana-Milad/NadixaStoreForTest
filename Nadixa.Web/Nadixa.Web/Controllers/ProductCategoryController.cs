using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class ProductCategoryController : Controller
    {
        private readonly NadixaDbContext _context;

        public ProductCategoryController(NadixaDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _context.ProductCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var prodCatViewModel = categories.Select(c => new ProductCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl
            }).ToList();

            return View(prodCatViewModel);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? imagePath = null;

            // Upload Image
            if (model.ImageFile != null)
            {
                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/categories");

                // create folder if not exists
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(model.ImageFile.FileName);

                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imagePath = "/images/categories/" + fileName;
            }

            var category = new ProductCategory
            {
                Name = model.Name,
                Description = model.Description,
                ImageUrl = imagePath
            };

            _context.ProductCategories.Add(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            var prodCatViewModel = new ProductCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl
            };

            return View(prodCatViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductCategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (category == null)
            {
                return NotFound();
            }

            // Upload New Image
            if (model.ImageFile != null)
            {
                string folder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/categories");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string fileName =
                    Guid.NewGuid().ToString()
                    + Path.GetExtension(model.ImageFile.FileName);

                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                category.ImageUrl = "/images/categories/" + fileName;
            }

            category.Name = model.Name;
            category.Description = model.Description;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category updated successfully";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            _context.ProductCategories.Remove(category);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully";

            return RedirectToAction(nameof(Index));
        }
    }
}
