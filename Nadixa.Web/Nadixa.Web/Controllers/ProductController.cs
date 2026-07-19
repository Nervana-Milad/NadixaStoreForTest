using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
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
        private readonly NadixaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUser> _userManager;
        private readonly string[] _allowedExtension = { ".jpg", ".jpeg", ".png", ".jfif" };
        private readonly StockNotificationService _stockNotificationService;
        private readonly IPermissionService _permissionService;


        public ProductController(NadixaDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager, StockNotificationService stockNotificationService, IPermissionService permissionService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _stockNotificationService = stockNotificationService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> Index(int? categoryId, int? subCategoryId, string? search, int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            IQueryable<Product> query = _context.Products.Include(p => p.ProductCategory);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.ProductCategoryId == categoryId.Value);
                var category = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Id == categoryId);
                ViewBag.CategoryName = category?.Name;
                ViewBag.CurrentCategoryId = categoryId;


                // هات الـ SubCategories بتاعت الـ Category دي
                ViewBag.SubCategories = await _context.ProductSubCategories
                    .Where(s => s.ProductCategoryId == categoryId)
                    .ToListAsync();
            }
            ViewBag.Categories = await _context.ProductCategories.ToListAsync();
            if (subCategoryId.HasValue)
            {
                query = query.Where(p => p.ProductSubCategoryId == subCategoryId.Value);
                ViewBag.CurrentSubCategoryId = subCategoryId;
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
                ViewBag.Search = search;
            }

            // 1. حساب إجمالي المنتجات المفلترة فقط قبل التقسيم
            int totalProducts = await query.CountAsync();
            int pageSize = 10;

            // 2. تطبيق الترتيب والتقسيم (Pagination)
            var products = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 3. إرسال معلومات الصفحات للـ View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            Dictionary<int, int> cartItems = new();

            if (user != null)
            {
                cartItems = await _context.Carts
                    .Include(c => c.Items)
                    .Where(c => c.UserId == user.Id)
                    .SelectMany(c => c.Items)
                    .ToDictionaryAsync(
                        i => i.ProductId,
                        i => i.Quantity
                    );
            }

            ViewBag.CartItems = cartItems;
            var notifyRequestedProductIds = new HashSet<int>();

            if (user != null)
            {
                notifyRequestedProductIds = (await _context.StockNotificationRequests
                    .Where(r => r.UserId == user.Id && !r.IsNotified)
                    .Select(r => r.ProductId)
                    .ToListAsync())
                    .ToHashSet();
            }

            ViewBag.NotifyRequestedProductIds = notifyRequestedProductIds;
            return View(products);
        }

        [HttpGet]
        public async Task<JsonResult> GetSubCategories(int categoryId)
        {
            var subCategories = await _context.ProductSubCategories
                .Where(s => s.ProductCategoryId == categoryId)
                .Select(s => new
                {
                    id = s.Id,
                    name = s.Name
                })
                .ToListAsync();

            return Json(subCategories);
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
            vm.Categories = await _context.ProductCategories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToListAsync();
        }

        private async Task LoadSubCategories(ProductCreateViewModel vm, int categoryId)
        {
            vm.SubCategories = await _context.ProductSubCategories
                .Where(s => s.ProductCategoryId == categoryId)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductCreateViewModel productViewModel)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories(productViewModel);

                if (productViewModel.CategoryId > 0)
                {
                    await LoadSubCategories(
                        productViewModel,
                        productViewModel.CategoryId);
                }

                return View(productViewModel);
            }

            if (productViewModel.MainImage == null)
            {
                ModelState.AddModelError("MainImage", "Main Image is required");

                await LoadCategories(productViewModel);

                if (productViewModel.CategoryId > 0)
                {
                    await LoadSubCategories(
                        productViewModel,
                        productViewModel.CategoryId);
                }

                return View(productViewModel);
            }

            var inputFileExtension =
                Path.GetExtension(productViewModel.MainImage.FileName).ToLower();

            bool isAllowed = _allowedExtension.Contains(inputFileExtension);

            if (!isAllowed)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid Image Format. Allowed formats are .jpg, .jpeg, .png, .jfif");

                await LoadCategories(productViewModel);

                if (productViewModel.CategoryId > 0)
                {
                    await LoadSubCategories(
                        productViewModel,
                        productViewModel.CategoryId);
                }

                return View(productViewModel);
            }

            string imagePath =
                await UploadFileToFolder(productViewModel.MainImage);

            var product = new Product
            {
                Name = productViewModel.Name,
                Description = productViewModel.Description,
                Price = productViewModel.Price,
                OldPrice = productViewModel.OldPrice,
                StockQuantity = productViewModel.StockQuantity,
                ProductCategoryId = productViewModel.CategoryId,
                ProductSubCategoryId = productViewModel.ProductSubCategoryId,
                MainImageUrlPath = imagePath
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Gallery Images
            if (productViewModel.GalleryImages != null &&
                productViewModel.GalleryImages.Any())
            {
                foreach (var image in productViewModel.GalleryImages)
                {
                    if (image == null)
                        continue;

                    var extension =
                        Path.GetExtension(image.FileName).ToLower();

                    if (!_allowedExtension.Contains(extension))
                        continue;

                    var path = await UploadFileToFolder(image);

                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = path,
                        IsMain = false
                    });
                }

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = AppMessages.ProductCreated;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products.Include(p => p.ProductCategory).Include(p => p.ProductSubCategory).Include(p => p.Colors).Include(p => p.Images).Include(p => p.Reviews).FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var reviews = product.Reviews.ToList();

            ViewBag.AvgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            ViewBag.TotalReviews = reviews.Count();
            ViewBag.CurrentUserId = user?.Id;

            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            ViewBag.SoldCount = await _context.OrderItems
                .Where(oi => oi.ProductId == id
                    && oi.Order.Status != OrderStatus.Cancelled
                    && oi.Order.CreatedAt >= oneMonthAgo)
                .SumAsync(oi => oi.Quantity);

            var vm = new ProductDetailViewModel
            {
                Product = product,
                ImageUrls = product.Images.Select(i => i.ImageUrl).ToList()
            };
            return View(vm);
        }


        [HttpGet]
        [RequirePermission("EditProductQuantity")]
        public async Task<IActionResult> Edit(int id)
        {
            var productFromDb = await _context.Products
                .Include(i => i.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productFromDb == null)
                return NotFound();

            var editViewModel = new ProductEditViewModel
            {
                Id = productFromDb.Id,
                Name = productFromDb.Name,
                Description = productFromDb.Description,
                Price = productFromDb.Price,
                OldPrice = productFromDb.OldPrice,
                StockQuantity = productFromDb.StockQuantity,
                IsFeatured = productFromDb.IsFeatured,
                ProductCategoryId = productFromDb.ProductCategoryId,
                ProductSubCategoryId = productFromDb.ProductSubCategoryId,
                ExistingMainImageUrl = productFromDb.MainImageUrlPath,

                ExistingImages = productFromDb.Images.Select(img => new ProductImageViewModel
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl
                }).ToList(),

                Categories = _context.ProductCategories.Select(cat => new SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = cat.Name
                }).ToList(),

                SubCategories = _context.ProductSubCategories
                .Where(s => s.ProductCategoryId == productFromDb.ProductCategoryId)
                .Select(subProd => new SelectListItem
                {
                    Value = subProd.Id.ToString(),
                    Text = subProd.Name
                }).ToList(),
            };

            return View(editViewModel);
        }


        [HttpPost]
        [RequirePermission("EditProductQuantity")]
        public async Task<IActionResult> Edit(ProductEditViewModel editViewModel)
        {

            var productFromDb = await _context.Products
        .Include(p => p.Images)
        .FirstOrDefaultAsync(p => p.Id == editViewModel.Id);

            if (productFromDb == null)
                return NotFound();

            bool canEditFull = User.IsInRole("Admin")
                || await _permissionService.UserHasPermissionAsync(User, "EditProducts");

            if (!canEditFull)
            {
                // 👇 غيّرنا الاسم من oldStock لـ previousStock
                int previousStock = productFromDb.StockQuantity;
                productFromDb.StockQuantity = editViewModel.StockQuantity;
                productFromDb.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                if (previousStock <= 0 && productFromDb.StockQuantity > 0)
                {
                    await _stockNotificationService.NotifySubscribersAsync(productFromDb.Id);
                }

                TempData["Success"] = "Stock quantity updated successfully.";
                return RedirectToAction("Detail", new { id = productFromDb.Id });
            }

            if (!ModelState.IsValid)
            {
                editViewModel.Categories = _context.ProductCategories.Select(cat => new SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = cat.Name
                }).ToList();

                editViewModel.SubCategories = _context.ProductSubCategories.Select(subProd => new SelectListItem
                {
                    Value = subProd.Id.ToString(),
                    Text = subProd.Name
                }).ToList();
                return View(editViewModel);
            }


            

            //var productFromDb = await _context.Products
            //    .Include(p => p.Images)
            //    .FirstOrDefaultAsync(p => p.Id == editViewModel.Id);

            //if (productFromDb == null)
            //    return NotFound();

            // Main Image
            if (editViewModel.MainImageUrl != null)
            {
                var inputFileExtension = Path.GetExtension(editViewModel.MainImageUrl.FileName).ToLower();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);

                if (!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid Image format.");
                    return View(editViewModel);
                }

                if (!string.IsNullOrEmpty(productFromDb.MainImageUrlPath))
                {
                    var existingFilePath = Path.Combine(
                        _webHostEnvironment.WebRootPath,
                        "Images",
                        Path.GetFileName(productFromDb.MainImageUrlPath));

                    if (System.IO.File.Exists(existingFilePath))
                    {
                        System.IO.File.Delete(existingFilePath);
                    }
                }

                productFromDb.MainImageUrlPath = await UploadFileToFolder(editViewModel.MainImageUrl);
            }

            // Deleted Images
            if (!string.IsNullOrEmpty(editViewModel.DeletedImages))
            {
                var idsToDelete = editViewModel.DeletedImages.Split(',').Select(int.Parse).ToList();
                var imagesToDelete = productFromDb.Images.Where(img => idsToDelete.Contains(img.Id)).ToList();

                foreach (var img in imagesToDelete)
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    _context.ProductImages.Remove(img);
                }
            }

            // New Gallery Images
            if (editViewModel.NewGalleryImages != null && editViewModel.NewGalleryImages.Count > 0)
            {
                foreach (var file in editViewModel.NewGalleryImages)
                {
                    var uploadedPath = await UploadFileToFolder(file);
                    productFromDb.Images.Add(new ProductImage
                    {
                        ImageUrl = uploadedPath,
                        ProductId = productFromDb.Id
                    });
                }
            }

            int oldStock = productFromDb.StockQuantity;   

            productFromDb.Name = editViewModel.Name;
            productFromDb.Price = editViewModel.Price;
            productFromDb.Description = editViewModel.Description;
            productFromDb.ProductCategoryId = editViewModel.ProductCategoryId;
            productFromDb.ProductSubCategoryId = editViewModel.ProductSubCategoryId;
            productFromDb.OldPrice = editViewModel.OldPrice;
            productFromDb.StockQuantity = editViewModel.StockQuantity;
            productFromDb.IsFeatured = editViewModel.IsFeatured;
            productFromDb.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            Console.WriteLine($"DEBUG: oldStock={oldStock}, newStock={productFromDb.StockQuantity}"); // 👈 ضيفي السطر ده مؤقتًا

            if (oldStock <= 0 && productFromDb.StockQuantity > 0)
            {
                Console.WriteLine("DEBUG: Condition met, calling NotifySubscribersAsync"); // 👈 وده كمان
                await _stockNotificationService.NotifySubscribersAsync(productFromDb.Id);
            }

            TempData["Success"] = AppMessages.ProductUpdated;
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> NotifyMeWhenAvailable(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    success = false,
                    requiresLogin = true,
                    message = "Please login to get notified when this product is back in stock."
                });
            }

            var user = await _userManager.GetUserAsync(User);

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found." });
            }

            if (product.StockQuantity > 0)
            {
                return Json(new { success = false, message = "This product is already in stock." });
            }

            var alreadyRequested = await _context.StockNotificationRequests
                .AnyAsync(r => r.ProductId == productId && r.UserId == user.Id && !r.IsNotified);

            if (alreadyRequested)
            {
                return Json(new { success = true, message = "You're already on the notify list for this item." });
            }

            _context.StockNotificationRequests.Add(new StockNotificationRequest
            {
                ProductId = productId,
                UserId = user.Id
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "We'll email you as soon as it's back in stock!" });
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productFromDb = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productFromDb == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(productFromDb.MainImageUrlPath))
            {
                var existingFilePath = Path.Combine(
                    _webHostEnvironment.WebRootPath,
                    "Images",
                    Path.GetFileName(productFromDb.MainImageUrlPath));

                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
            }

            _context.Products.Remove(productFromDb);
            await _context.SaveChangesAsync(); // ← المفروض تكون هنا الأول

            TempData["Success"] = AppMessages.ProductDeleted;
            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> AddReview([FromBody] Review review)
        {
            if (review == null)
                return Json(new { success = false, message = "Invalid review data" });

            if (review.ProductId <= 0)
                return Json(new { success = false, message = "Invalid Product" });

            if (review.Rating < 1 || review.Rating > 5)
                return Json(new { success = false, message = "Rating must be between 1 and 5" });

            if (string.IsNullOrWhiteSpace(review.Content))
                return Json(new { success = false, message = "Review content cannot be empty" });

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Json(new { success = false, message = "User Not Found" });

            bool alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.ProductId == review.ProductId && r.UserId == user.Id);

            if (alreadyReviewed)
                return Json(new { success = false, message = "You have already reviewed this product" });

            var newReview = new Review
            {
                ProductId = review.ProductId,
                Rating = review.Rating,
                Content = review.Content,
                CreatedAt = DateTime.Now,
                UserId = user.Id,
                UserName = user.FirstName + " " + user.LastName,
                UserImage = "/images/avatar-01.jpg"
            };

            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();

            var avgRating = await _context.Reviews
                .Where(r => r.ProductId == review.ProductId)
                .AverageAsync(r => r.Rating);

            return Json(new
            {
                success = true,
                id = newReview.Id,
                username = newReview.UserName,
                content = newReview.Content,
                rating = newReview.Rating,
                userImage = newReview.UserImage,
                userId = newReview.UserId,
                avgRating = avgRating
            });
        }



        private async Task<string> UploadFileToFolder(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception("Invalid file.");

            var inputFileExtension = Path.GetExtension(file.FileName);

            var fileName = Guid.NewGuid().ToString() + inputFileExtension;

            var wwwRootPath = _webHostEnvironment.WebRootPath;
            var imagesFolderPath = Path.Combine(wwwRootPath, "images", "products");

            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            var filePath = Path.Combine(imagesFolderPath, fileName);

            // 🔹 تحديد حد أقصى للحجم (مثلاً 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File size exceeds 5MB limit.");


            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                return "Error Uploading Images: " + ex.Message;
            }
            return "/images/products/" + fileName;
        }

        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images) // لو عندك table للصور
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (product == null) return NotFound();

            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            var soldLastMonth = await _context.OrderItems
                .Where(oi => oi.ProductId == id
                    && oi.Order.Status != OrderStatus.Cancelled
                    && oi.Order.CreatedAt >= oneMonthAgo)
                .SumAsync(oi => oi.Quantity);

            var model = new ProductQuickViewViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                MainImageUrl = product.MainImageUrlPath,
                StockQuantity = product.StockQuantity,
                Images = product.Images.ToList(),
                SoldLastMonth = soldLastMonth
            };


            return PartialView("_QuickViewModal", model);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized();

            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
                return NotFound();

            // 🔥 مهم: تأكدي إن ده صاحب الريفيو
            if (review.UserId != user.Id && !User.IsInRole("Admin"))
                return Forbid();

            int productId = review.ProductId;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            var avgRating = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Select(r => (double?)r.Rating)
                .AverageAsync() ?? 0;

            var reviewsCount = await _context.Reviews
                .CountAsync(r => r.ProductId == productId);

            return Json(new
            {
                success = true,
                avgRating,
                reviewsCount
            });

            //return RedirectToAction("Detail", new { id = review.ProductId });
        }


        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            var user = await _userManager.GetUserAsync(User);

            Dictionary<int, int> cartItems = new();
            HashSet<int> notifyRequestedIds = new();


            if (user != null)
            {
                cartItems = await _context.Carts
                    .Where(c => c.UserId == user.Id)
                    .SelectMany(c => c.Items)
                    .ToDictionaryAsync(
                        i => i.ProductId,
                        i => i.Quantity
                    );

                notifyRequestedIds = (await _context.StockNotificationRequests
                    .Where(r => r.UserId == user.Id && !r.IsNotified)
                    .Select(r => r.ProductId)
                    .ToListAsync())
                    .ToHashSet();
            }

            var products = await _context.Products
                .Include(p => p.ProductCategory)
                .AsNoTracking()
                .Where(p => string.IsNullOrEmpty(term)
                    || p.Name.Contains(term))
                .ToListAsync();

            var result = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                oldPrice = p.OldPrice,
                stockQuantity = p.StockQuantity,
                description = p.Description.Length > 50
                    ? p.Description.Substring(0, 50) + "..."
                    : p.Description,
                mainImageUrlPath = p.MainImageUrlPath,
                categoryName = p.ProductCategory.Name,
                cartQuantity = cartItems.ContainsKey(p.Id)
                    ? cartItems[p.Id]
                    : 0,
                notifyRequested = notifyRequestedIds.Contains(p.Id)

            });

            return Json(result);
        }



    }
}
