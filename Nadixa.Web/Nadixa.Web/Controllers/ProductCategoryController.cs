using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.ProductCategory;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class ProductCategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public ProductCategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();

            var viewModel = categories.Select(c => new ProductCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl
            }).ToList();

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new CategoryCreateDto
            {
                Name = model.Name,
                Description = model.Description,
                Image = model.ImageFile != null
                    ? new FileUploadRequest
                    {
                        Content = model.ImageFile.OpenReadStream(),
                        FileName = model.ImageFile.FileName,
                        Length = model.ImageFile.Length
                    }
                    : null
            };

            await _categoryService.CreateAsync(dto);

            TempData["Success"] = AppMessages.ProductCatCreated;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
                return NotFound();

            var viewModel = new ProductCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(ProductCategoryViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new CategoryEditDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                NewImage = model.ImageFile != null
                    ? new FileUploadRequest
                    {
                        Content = model.ImageFile.OpenReadStream(),
                        FileName = model.ImageFile.FileName,
                        Length = model.ImageFile.Length
                    }
                    : null
            };

            var updated = await _categoryService.UpdateAsync(dto);
            if (!updated)
                return NotFound();

            TempData["Success"] = AppMessages.ProductCatUpdated;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.DeleteAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = AppMessages.ProductCatDeleted;
            return RedirectToAction(nameof(Index));
        }
    }
}