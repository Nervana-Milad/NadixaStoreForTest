using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.ProductSubCategory;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class ProductSubCategoryController : Controller
    {
        private readonly ISubCategoryService _subCategoryService;
        private readonly ICategoryService _categoryService;

        public ProductSubCategoryController(ISubCategoryService subCategoryService, ICategoryService categoryService)
        {
            _subCategoryService = subCategoryService;
            _categoryService = categoryService;
        }

        // INDEX
        public async Task<IActionResult> Index()
        {
            var subCategories = await _subCategoryService.GetAllAsync();
            return View(subCategories);
        }

        // CREATE GET
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductSubCategoryViewModel
            {
                Categories = await LoadCategoryOptionsAsync()
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
                model.Categories = await LoadCategoryOptionsAsync();
                return View(model);
            }

            var dto = new SubCategoryCreateDto
            {
                Name = model.Name,
                Description = model.Description,
                ProductCategoryId = model.ProductCategoryId,
                Image = model.ImageFile != null
                    ? new FileUploadRequest
                    {
                        Content = model.ImageFile.OpenReadStream(),
                        FileName = model.ImageFile.FileName,
                        Length = model.ImageFile.Length
                    }
                    : null
            };

            await _subCategoryService.CreateAsync(dto);

            TempData["Success"] = AppMessages.ProductSubCatCreated;
            return RedirectToAction(nameof(Index));
        }

        // EDIT GET
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var subCategory = await _subCategoryService.GetByIdAsync(id);
            if (subCategory == null)
                return NotFound();

            var vm = new ProductSubCategoryViewModel
            {
                Id = subCategory.Id,
                Name = subCategory.Name,
                Description = subCategory.Description,
                ImageUrl = subCategory.ImageUrl,
                ProductCategoryId = subCategory.ProductCategoryId,
                Categories = await LoadCategoryOptionsAsync()
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
                model.Categories = await LoadCategoryOptionsAsync();
                return View(model);
            }

            var dto = new SubCategoryEditDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                ProductCategoryId = model.ProductCategoryId,
                NewImage = model.ImageFile != null
                    ? new FileUploadRequest
                    {
                        Content = model.ImageFile.OpenReadStream(),
                        FileName = model.ImageFile.FileName,
                        Length = model.ImageFile.Length
                    }
                    : null
            };

            var updated = await _subCategoryService.UpdateAsync(dto);
            if (!updated)
                return NotFound();

            TempData["Success"] = AppMessages.ProductSubCatUpdated;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _subCategoryService.DeleteAsync(id);

            if (!result.Success)
            {
                TempData["Error"] = result.ErrorMessage;
                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = AppMessages.ProductSubCatDeleted;
            return RedirectToAction(nameof(Index));
        }

        // ===== Helper Method =====
        private async Task<List<ProductCategoryViewModel>> LoadCategoryOptionsAsync()
        {
            var categories = await _categoryService.GetAllAsync();
            return categories.Select(c => new ProductCategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();
        }
    }
}
