using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.Product;
using Nadixa.Application.DTOS.Review;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Infrastructure.Services;
using Nadixa.Web.Filters;
using Nadixa.Web.Helpers;
using Nadixa.Web.Models.ViewModels;
using System.Threading.Tasks;



namespace Nadixa.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;



        public ProductController(
            UserManager<AppUser> userManager, 
            IPermissionService permissionService, 
            IProductService productService)
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _productService = productService; 
        }


        public async Task<IActionResult> Index(int? categoryId, int? subCategoryId, string? search, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            int pageSize = 10;
            var result = await _productService.GetProductsAsync(
                categoryId, subCategoryId, search, page, pageSize, user?.Id);

            ViewBag.CategoryName = categoryId.HasValue
                ? result.Categories.FirstOrDefault(c => c.Id == categoryId.Value)?.Name
                : null;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.SubCategories = result.SubCategories;
            ViewBag.Categories = result.Categories;
            ViewBag.CurrentSubCategoryId = subCategoryId;
            ViewBag.Search = search;
            ViewBag.CurrentPage = result.Page;
            ViewBag.TotalPages = result.TotalPages;

            // لو الطلب جاي بـ AJAX، نرجّع بس الجزء اللي اتغيّر (بدون الصفحة كاملة)
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductsFilterAndList", result.Products);
            }

            return View(result.Products);
        }
        

        [HttpGet]
        public async Task<JsonResult> GetSubCategories(int categoryId)
        {
            var subCategories = await _productService.GetSubCategoriesAsync(categoryId);
            var result = subCategories.Select(s => new { id = s.Id, name = s.Name });
            return Json(result);
        }




        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var vm = new ProductCreateViewModel();
            await LoadCategories(vm);
            vm.SubCategories = new List<SelectListItem>();
            return View(vm);
        }

        private async Task LoadCategories(ProductCreateViewModel vm)
        {
            var categories = await _productService.GetCategoriesAsync();
            vm.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }

        private async Task LoadSubCategories(ProductCreateViewModel vm, int categoryId)
        {
            var subCategories = await _productService.GetSubCategoriesAsync(categoryId);
            vm.SubCategories = subCategories.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            }).ToList();
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductCreateViewModel productViewModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories(productViewModel);
                if (productViewModel.CategoryId > 0)
                    await LoadSubCategories(productViewModel, productViewModel.CategoryId);
                return View(productViewModel);
            }

            if (productViewModel.MainImage == null)
            {
                ModelState.AddModelError("MainImage", "Main Image is required");
                await LoadCategories(productViewModel);
                if (productViewModel.CategoryId > 0)
                    await LoadSubCategories(productViewModel, productViewModel.CategoryId);
                return View(productViewModel);
            }

            var dto = new ProductCreateDto
            {
                Name = productViewModel.Name,
                Description = productViewModel.Description,
                Price = productViewModel.Price,
                OldPrice = productViewModel.OldPrice,
                StockQuantity = productViewModel.StockQuantity,
                CategoryId = productViewModel.CategoryId,
                ProductSubCategoryId = productViewModel.ProductSubCategoryId,
                MainImage = new FileUploadRequest
                {
                    Content = productViewModel.MainImage.OpenReadStream(),
                    FileName = productViewModel.MainImage.FileName,
                    Length = productViewModel.MainImage.Length
                },
                GalleryImages = productViewModel.GalleryImages?.Select(f => new FileUploadRequest
                {
                    Content = f.OpenReadStream(),
                    FileName = f.FileName,
                    Length = f.Length
                }).ToList()
            };

            try
            {
                await _productService.CreateProductAsync(dto);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadCategories(productViewModel);
                if (productViewModel.CategoryId > 0)
                    await LoadSubCategories(productViewModel, productViewModel.CategoryId);
                return View(productViewModel);
            }

            TempData["Success"] = AppMessages.ProductCreated;
            return RedirectToAction("Index");
        }





        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var product = await _productService.GetProductDetailAsync(id, user?.Id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.CurrentUserId = user?.Id;

            return View(product);
        }

        [HttpGet]
        [RequirePermission("EditProductQuantity")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductForEditAsync(id);
            if (product == null)
                return NotFound();

            var categories = await _productService.GetCategoriesAsync();
            var subCategories = await _productService.GetSubCategoriesAsync(product.ProductCategoryId);

            var editViewModel = new ProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OldPrice = product.OldPrice,
                StockQuantity = product.StockQuantity,
                IsFeatured = product.IsFeatured,
                ProductCategoryId = product.ProductCategoryId,
                ProductSubCategoryId = product.ProductSubCategoryId,
                ExistingMainImageUrl = product.MainImageUrl,
                ExistingImages = product.GalleryImages.Select(img => new ProductImageViewModel
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl
                }).ToList(),
                Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }),
                SubCategories = subCategories.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            };

            return View(editViewModel);
        }

        [HttpPost]
        [RequirePermission("EditProductQuantity")]
        public async Task<IActionResult> Edit(ProductEditViewModel editViewModel)
        {
            bool canEditFull = User.IsInRole("Admin")
                || await _permissionService.UserHasPermissionAsync(User, "EditProducts");

            if (!canEditFull)
            {
                var quickEditDto = new ProductEditDto
                {
                    Id = editViewModel.Id,
                    StockQuantity = editViewModel.StockQuantity
                };

                var updated = await _productService.UpdateStockOnlyAsync(editViewModel.Id, editViewModel.StockQuantity);

                if (!updated)
                    return NotFound();

                TempData["Success"] = "Stock quantity updated successfully.";
                return RedirectToAction("Detail", new { id = editViewModel.Id });
            }

            if (!ModelState.IsValid)
            {
                var categories = await _productService.GetCategoriesAsync();
                var subCategories = await _productService.GetSubCategoriesAsync(editViewModel.ProductCategoryId);

                editViewModel.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                editViewModel.SubCategories = subCategories.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
                return View(editViewModel);
            }

            var dto = new ProductEditDto
            {
                Id = editViewModel.Id,
                Name = editViewModel.Name,
                Description = editViewModel.Description,
                Price = editViewModel.Price,
                OldPrice = editViewModel.OldPrice,
                StockQuantity = editViewModel.StockQuantity,
                IsFeatured = editViewModel.IsFeatured,
                ProductCategoryId = editViewModel.ProductCategoryId,
                ProductSubCategoryId = editViewModel.ProductSubCategoryId,
                NewMainImage = editViewModel.MainImageUrl != null
                    ? new FileUploadRequest
                    {
                        Content = editViewModel.MainImageUrl.OpenReadStream(),
                        FileName = editViewModel.MainImageUrl.FileName,
                        Length = editViewModel.MainImageUrl.Length
                    }
                    : null,
                NewGalleryImages = editViewModel.NewGalleryImages?.Select(f => new FileUploadRequest
                {
                    Content = f.OpenReadStream(),
                    FileName = f.FileName,
                    Length = f.Length
                }).ToList(),
                DeletedImageIds = !string.IsNullOrEmpty(editViewModel.DeletedImages)
                    ? editViewModel.DeletedImages.Split(',').Select(int.Parse).ToList()
                    : null
            };

            bool success;
            try
            {
                success = await _productService.UpdateProductAsync(dto);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var categories = await _productService.GetCategoriesAsync();
                var subCategories = await _productService.GetSubCategoriesAsync(editViewModel.ProductCategoryId);
                editViewModel.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
                editViewModel.SubCategories = subCategories.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name });
                return View(editViewModel);
            }

            if (!success)
                return NotFound();

            TempData["Success"] = AppMessages.ProductUpdated;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> NotifyMeWhenAvailable(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            var (success, requiresLogin, message) = await _productService.RequestNotifyAsync(productId, user?.Id);

            return Json(new { success, requiresLogin, message });
        }
        


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _productService.DeleteProductAsync(id);

            if (!deleted)
                return NotFound();

            TempData["Success"] = AppMessages.ProductDeleted;
            return RedirectToAction("Index", "Product");
        }




        [HttpPost]
        [Authorize]
        public async Task<JsonResult> AddReview([FromBody] ReviewCreateDto reviewDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User Not Found" });

            var result = await _productService.AddReviewAsync(reviewDto, user.Id, $"{user.FirstName} {user.LastName}");

            if (!result.Success)
                return Json(new { success = false, message = result.Message });

            return Json(new
            {
                success = true,
                id = result.Review!.Id,
                username = result.Review.UserName,
                content = result.Review.Content,
                rating = result.Review.Rating,
                userImage = "/images/avatar-01.jpg",
                userId = result.Review.UserId,
                avgRating = result.AvgRating
            });
        }

        

        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {

            var user = await _userManager.GetUserAsync(User);

            var product = await _productService.GetProductDetailAsync(id, user?.Id);

            if (product == null)
                return NotFound();

            return PartialView("_QuickViewModal", product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _productService.DeleteReviewAsync(id, user.Id, User.IsInRole("Admin"));

            if (!result.Success)
            {
                if (result.Message == "Review not found") return NotFound();
                if (result.Message == "Forbidden") return Forbid();
                return BadRequest(result.Message);
            }
            return Json(new
            {
                success = true,
                avgRating = result.AvgRating,
                reviewsCount = result.ReviewsCount
            });
        }


    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        var user = await _userManager.GetUserAsync(User);

        var result = await _productService.SearchProductsAsync(term, user?.Id);

        if (!result.Any())
        {
            return Content("<div class=\"col-12 text-center\"><p>No products found</p></div>", "text/html");
        }

        var htmlBuilder = new System.Text.StringBuilder();

        foreach (var p in result)
        {
            var cardVm = new ProductCardViewModel
            {
                Product = new ProductListItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    StockQuantity = p.StockQuantity,
                    PictureUrl = p.MainImageUrlPath ?? string.Empty,
                    Category = p.CategoryName,
                    BadgeText = p.BadgeText,
                    BadgeColorHex = p.BadgeColorHex,
                    DiscountedPrice = p.DiscountedPrice,
                    AvgRating = p.AvgRating,
                    ReviewsCount = p.ReviewsCount,
                    CartQuantity = p.CartQuantity,
                    NotifyRequested = p.NotifyRequested
                },
                CartQuantity = p.CartQuantity,
                NotifyRequested = p.NotifyRequested
            };

            var cardHtml = await this.RenderPartialViewToStringAsync("_ProductCard", cardVm);

            // نلف كل كارد في نفس wrapper div المستخدم في الـ Index/الـ JS القديم
            htmlBuilder.Append($"<div class=\"col-sm-6 col-md-4 col-lg-3 isotope-item bag pb-3\">{cardHtml}</div>");
        }

        return Content(htmlBuilder.ToString(), "text/html");
    }
    

}
}
