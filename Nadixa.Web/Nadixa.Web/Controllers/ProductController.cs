using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Application.DTOS;
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
        private readonly IProductService _productService;   // 👈 جديد



        public ProductController(
            UserManager<AppUser> userManager, 
            IPermissionService permissionService, 
            IProductService productService)
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _productService = productService;   // 👈 جديد
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
                // مسموح بس بتعديل الكمية
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
        
        //[HttpPost]
        //public async Task<IActionResult> NotifyMeWhenAvailable(int productId)
        //{
        //    if (!User.Identity.IsAuthenticated)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            requiresLogin = true,
        //            message = "Please login to get notified when this product is back in stock."
        //        });
        //    }

        //    var user = await _userManager.GetUserAsync(User);

        //    var product = await _context.Products.FindAsync(productId);
        //    if (product == null)
        //    {
        //        return Json(new { success = false, message = "Product not found." });
        //    }

        //    if (product.StockQuantity > 0)
        //    {
        //        return Json(new { success = false, message = "This product is already in stock." });
        //    }

        //    var alreadyRequested = await _context.StockNotificationRequests
        //        .AnyAsync(r => r.ProductId == productId && r.UserId == user.Id && !r.IsNotified);

        //    if (alreadyRequested)
        //    {
        //        return Json(new { success = true, message = "You're already on the notify list for this item." });
        //    }

        //    _context.StockNotificationRequests.Add(new StockNotificationRequest
        //    {
        //        ProductId = productId,
        //        UserId = user.Id
        //    });

        //    await _context.SaveChangesAsync();

        //    return Json(new { success = true, message = "We'll email you as soon as it's back in stock!" });
        //}

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

        //[HttpPost]
        //[Authorize]
        //public async Task<JsonResult> AddReview([FromBody] Review review)
        //{
        //    if (review == null)
        //        return Json(new { success = false, message = "Invalid review data" });

        //    if (review.ProductId <= 0)
        //        return Json(new { success = false, message = "Invalid Product" });

        //    if (review.Rating < 1 || review.Rating > 5)
        //        return Json(new { success = false, message = "Rating must be between 1 and 5" });

        //    if (string.IsNullOrWhiteSpace(review.Content))
        //        return Json(new { success = false, message = "Review content cannot be empty" });

        //    var user = await _userManager.GetUserAsync(User);

        //    if (user == null)
        //        return Json(new { success = false, message = "User Not Found" });

        //    bool alreadyReviewed = await _context.Reviews
        //        .AnyAsync(r => r.ProductId == review.ProductId && r.UserId == user.Id);

        //    if (alreadyReviewed)
        //        return Json(new { success = false, message = "You have already reviewed this product" });

        //    var newReview = new Review
        //    {
        //        ProductId = review.ProductId,
        //        Rating = review.Rating,
        //        Content = review.Content,
        //        CreatedAt = DateTime.Now,
        //        UserId = user.Id,
        //        UserName = user.FirstName + " " + user.LastName,
        //        UserImage = "/images/avatar-01.jpg"
        //    };

        //    _context.Reviews.Add(newReview);
        //    await _context.SaveChangesAsync();

        //    var avgRating = await _context.Reviews
        //        .Where(r => r.ProductId == review.ProductId)
        //        .AverageAsync(r => r.Rating);

        //    return Json(new
        //    {
        //        success = true,
        //        id = newReview.Id,
        //        username = newReview.UserName,
        //        content = newReview.Content,
        //        rating = newReview.Rating,
        //        userImage = newReview.UserImage,
        //        userId = newReview.UserId,
        //        avgRating = avgRating
        //    });
        //}



        //private async Task<string> UploadFileToFolder(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        throw new Exception("Invalid file.");

        //    var inputFileExtension = Path.GetExtension(file.FileName);

        //    var fileName = Guid.NewGuid().ToString() + inputFileExtension;

        //    var wwwRootPath = _webHostEnvironment.WebRootPath;
        //    var imagesFolderPath = Path.Combine(wwwRootPath, "images", "products");

        //    if (!Directory.Exists(imagesFolderPath))
        //    {
        //        Directory.CreateDirectory(imagesFolderPath);
        //    }

        //    var filePath = Path.Combine(imagesFolderPath, fileName);

        //    // 🔹 تحديد حد أقصى للحجم (مثلاً 5MB)
        //    if (file.Length > 5 * 1024 * 1024)
        //        throw new Exception("File size exceeds 5MB limit.");


        //    try
        //    {
        //        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(fileStream);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new InvalidOperationException("Error Uploading Images:", ex);
        //    }
        //    return "/images/products/" + fileName;
        //}




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
        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> DeleteReview(int id)
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    if (user == null)
        //        return Unauthorized();

        //    var review = await _context.Reviews.FindAsync(id);

        //    if (review == null)
        //        return NotFound();

        //    // 🔥 مهم: تأكدي إن ده صاحب الريفيو
        //    if (review.UserId != user.Id && !User.IsInRole("Admin"))
        //        return Forbid();

        //    int productId = review.ProductId;

        //    _context.Reviews.Remove(review);
        //    await _context.SaveChangesAsync();

        //    var avgRating = await _context.Reviews
        //        .Where(r => r.ProductId == productId)
        //        .Select(r => (double?)r.Rating)
        //        .AverageAsync() ?? 0;

        //    var reviewsCount = await _context.Reviews
        //        .CountAsync(r => r.ProductId == productId);

        //    return Json(new
        //    {
        //        success = true,
        //        avgRating,
        //        reviewsCount
        //    });

        //    //return RedirectToAction("Detail", new { id = review.ProductId });
        //}


        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _productService.SearchProductsAsync(term, user?.Id);

            var json = result.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                oldPrice = p.OldPrice,
                stockQuantity = p.StockQuantity,
                description = p.Description,
                mainImageUrlPath = p.MainImageUrlPath,
                categoryName = p.CategoryName,
                cartQuantity = p.CartQuantity,
                notifyRequested = p.NotifyRequested,
                badgeText = p.BadgeText,
                badgeColorHex = p.BadgeColorHex,
                discountedPrice = p.DiscountedPrice
            });

            return Json(json);
        }

        //[HttpGet]
        //public async Task<IActionResult> Search(string term)
        //{
        //    var user = await _userManager.GetUserAsync(User);

        //    Dictionary<int, int> cartItems = new();
        //    HashSet<int> notifyRequestedIds = new();


        //    if (user != null)
        //    {
        //        cartItems = await _context.Carts
        //            .Where(c => c.UserId == user.Id)
        //            .SelectMany(c => c.Items)
        //            .ToDictionaryAsync(
        //                i => i.ProductId,
        //                i => i.Quantity
        //            );

        //        notifyRequestedIds = (await _context.StockNotificationRequests
        //            .Where(r => r.UserId == user.Id && !r.IsNotified)
        //            .Select(r => r.ProductId)
        //            .ToListAsync())
        //            .ToHashSet();
        //    }

        //    var products = await _context.Products
        //        .Include(p => p.ProductCategory)
        //        .AsNoTracking()
        //        .Where(p => string.IsNullOrEmpty(term)
        //            || p.Name.Contains(term))
        //        .ToListAsync();

        //    var activePromotions = await _promotionService.GetActivePromotionsAsync();

        //    var productPromotions = new Dictionary<int, ProductPromoInfo>();

        //    foreach (var product in products)
        //    {
        //        var promo = activePromotions
        //            .Where(p =>
        //                !p.IsFirstPurchaseOnly &&
        //                (p.Scope == PromotionScope.AllProducts ||
        //                 (p.Scope == PromotionScope.Category && p.ProductCategoryId == product.ProductCategoryId) ||
        //                 (p.Scope == PromotionScope.SubCategory && p.ProductSubCategoryId == product.ProductSubCategoryId) ||
        //                 (p.Scope == PromotionScope.SpecificProduct && p.ProductId == product.Id)))
        //            .OrderByDescending(p => p.Priority)
        //            .FirstOrDefault();
        //        if (promo == null) continue;

        //        productPromotions[product.Id] = new ProductPromoInfo
        //        {
        //            BadgeText = promo.BadgeText,
        //            BadgeColorHex = promo.BadgeColorHex,
        //            DiscountedPrice = promo.Type == PromotionType.BuyXGetYFree
        //        ? null
        //        : _promotionService.CalculateDiscountedPrice(product.Price, promo)
        //        };
        //    }

        //        var result = products.Select(p => new
        //    {
        //        id = p.Id,
        //        name = p.Name,
        //        price = p.Price,
        //        oldPrice = p.OldPrice,
        //        stockQuantity = p.StockQuantity,
        //        description = string.IsNullOrEmpty(p.Description) ? ""
        //            :(p.Description.Length > 50 ? p.Description.Substring(0, 50) + "..." : p.Description),
        //        mainImageUrlPath = p.MainImageUrlPath,
        //        categoryName = p.ProductCategory.Name,
        //        cartQuantity = cartItems.ContainsKey(p.Id)
        //            ? cartItems[p.Id]
        //            : 0,
        //        notifyRequested = notifyRequestedIds.Contains(p.Id),
        //        badgeText = productPromotions.ContainsKey(p.Id)
        //            ? productPromotions[p.Id].BadgeText
        //            : null,
        //        badgeColorHex = productPromotions.ContainsKey(p.Id)
        //            ? productPromotions[p.Id].BadgeColorHex
        //            : null,
        //        discountedPrice = productPromotions.ContainsKey(p.Id)
        //            ? productPromotions[p.Id].DiscountedPrice
        //            : null

        //        });

        //    return Json(result);
        //}



    }
}
