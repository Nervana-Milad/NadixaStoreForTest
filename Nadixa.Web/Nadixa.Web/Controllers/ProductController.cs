using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;
using static System.Net.Mime.MediaTypeNames;



namespace Nadixa.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<AppUser> _userManager;
        private readonly string[] _allowedExtension = { ".jpg", ".jpeg", ".png", ".jfif" };

        public ProductController(NadixaDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<AppUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? categotyId, string? search)
        {
            var user = await _userManager.GetUserAsync(User);
            IQueryable<Product> query = _context.Products.Include(p => p.Category);

            if (categotyId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categotyId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if(user != null)
            {
                ViewBag.WishlistIds = await _context.WishlistItems.Where(w => w.Wishlist.UserId == user.Id).Select(w => w.ProductId).ToListAsync();
            }

            var products = await query.OrderByDescending(p => p.Id).ToListAsync();

            return View(products);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var productViewModel = new ProductViewModel();
            productViewModel.Categories = _context.Categories.Select(c =>
            new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }
            ).ToList();

           
            return View(productViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ProductViewModel productViewModel)
        {
            
            if (ModelState.IsValid)
            {
                var inputFileExtension = Path.GetExtension(productViewModel.MainImage.FileName).ToLower();
                bool isAllowed = _allowedExtension.Contains(inputFileExtension);

                if(!isAllowed)
                {
                    ModelState.AddModelError("", "Invalid Image Format. Allowed formats are .jpg, .jpeg, .png, .jfif");
                    return View(productViewModel);
                }

                productViewModel.Product.MainImageUrlPath = await UploadFileToFolder(productViewModel.MainImage);
                await _context.Products.AddAsync(productViewModel.Product);
                await _context.SaveChangesAsync();

                if(productViewModel.GalleryImages !=null && productViewModel.GalleryImages.Any())
                {
                    foreach(var image in productViewModel.GalleryImages)
                    {
                        if (image == null) continue;
                        var extension = Path.GetExtension(image.FileName).ToLower();

                        if (!_allowedExtension.Contains(extension)) continue;

                        var imagePath = await UploadFileToFolder(image);

                        _context.ProductImages.Add(new ProductImage
                        {
                            ProductId = productViewModel.Product.Id,
                            ImageUrl = imagePath,
                            IsMain = false
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("Index", "Home");
            }

            productViewModel.Categories = _context.Categories.Select(c =>
            new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }
            ).ToList();
            return View(productViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                ViewBag.WishlistIds = await _context.WishlistItems
                    .Where(w => w.Wishlist.UserId == user.Id)
                    .Select(w => w.ProductId)
                    .ToListAsync();
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = _context.Products.Include(p => p.Category).Include(p => p.Colors).Include(p => p.Images).Include(p => p.Reviews).FirstOrDefault(p => p.Id == id);

            if(product == null)
            {
                return NotFound();
            }

            var vm = new ProductDetailViewModel
            {
                Product = product,
                ImageUrls = product.Images.Select(i => i.ImageUrl).ToList()
            };
            return View(vm);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var productFromDb = await _context.Products.Include(i => i.Images).FirstOrDefaultAsync(p => p.Id == id);

            if(productFromDb == null)
            {
                return NotFound();
            }
            EditViewModel editViewModel = new EditViewModel
            {
                Product = productFromDb,
                ExistingImages = productFromDb.Images.Select(img => new ProductImageViewModel
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl
                }).ToList(),

                Categories = _context.Categories.Select(cat =>
                new SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = cat.Name
                }).ToList()
            };

            return View(editViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(EditViewModel editViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(editViewModel);
            }
            var productFromDb = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == editViewModel.Product.Id);
            
            if(productFromDb == null)
            {
                return NotFound();
            }

            if(editViewModel.MainImageUrl != null)
            {
                var inputFileExtension = Path.GetExtension(editViewModel.MainImageUrl.FileName).ToLower();
                bool isAlllowed = _allowedExtension.Contains(inputFileExtension);

                if (!isAlllowed)
                {
                    ModelState.AddModelError("", "Invalid Image format. Allowed Formats are .jpg, .jpeg, .png, .jfif");
                    return View(editViewModel);
                }
                var existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", Path.GetFileName(productFromDb.MainImageUrlPath));

                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
                editViewModel.Product.MainImageUrlPath = await UploadFileToFolder(editViewModel.MainImageUrl);
            }
            else
            {
                editViewModel.Product.MainImageUrlPath = productFromDb.MainImageUrlPath;
            }

            if (!string.IsNullOrEmpty(editViewModel.DeletedImages))
            {
                var idsToDelete = editViewModel.DeletedImages.Split(',').Select(int.Parse).ToList();
                var imagesToDelete = productFromDb.Images.Where(img => idsToDelete.Contains(img.Id)).ToList();

                foreach(var img in imagesToDelete)
                {
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, img.ImageUrl.TrimStart('/'));
                    if(System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    _context.ProductImages.Remove(img);
                }
            }

            if(editViewModel.NewGalleryImages != null && editViewModel.NewGalleryImages.Count > 0)
            {
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
            }

            //editViewModel.Product.UpdatedAt = DateTime.Now;

            productFromDb.Name = editViewModel.Product.Name;
            productFromDb.Price = editViewModel.Product.Price;
            productFromDb.Description = editViewModel.Product.Description;
            productFromDb.CategoryId = editViewModel.Product.CategoryId;
            productFromDb.OldPrice = editViewModel.Product.OldPrice;
            productFromDb.StockQuantity = editViewModel.Product.StockQuantity;
            productFromDb.MainImageUrlPath = editViewModel.Product.MainImageUrlPath;

            productFromDb.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productFromDb = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (string.IsNullOrEmpty(productFromDb.MainImageUrlPath))
            {
                var existingFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Images", Path.GetFileName(productFromDb.MainImageUrlPath));
                if (System.IO.File.Exists(existingFilePath))
                {
                    System.IO.File.Delete(existingFilePath);
                }
            }
            _context.Products.Remove(productFromDb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [Authorize]
        public JsonResult AddReview([FromBody]Review review)
        {
            if (review == null)
            {
                return Json(new { error = "Review data was null" });
            }

            review.CreatedAt = DateTime.Now;
            review.UserImage = "/images/avatar-01.jpg";
            _context.Reviews.Add(review);
            _context.SaveChanges();

            return Json(new
            {
                username = review.UserName,
                content = review.Content,
                rating = review.Rating,
                userImage = review.UserImage
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

            if(!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            var filePath = Path.Combine(imagesFolderPath, fileName);

            // 🔹 تحديد حد أقصى للحجم (مثلاً 5MB)
            if (file.Length > 5 * 1024 * 1024)
                throw new Exception("File size exceeds 5MB limit.");


            try
            {
                await using(var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch(Exception ex)
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

            var model = new ProductQuickViewViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                MainImageUrl = product.MainImageUrlPath,
                Images = product.Images.ToList()
            };


            return PartialView("_QuickViewModal", model);
        }

    }
}
