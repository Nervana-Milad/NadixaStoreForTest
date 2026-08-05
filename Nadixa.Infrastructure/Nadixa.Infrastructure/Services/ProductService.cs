using Nadixa.Application.DTOS;
using Nadixa.Core.Entities;
using Nadixa.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Nadixa.Application.Interfaces;
using Nadixa.Application.DTOS.Review;
using Nadixa.Application.DTOS.Product;
namespace Nadixa.Infrastructure.Services
{
    public class ProductService :IProductService
    {
        private readonly Core.Interfaces.IUnitOfWork _unitOfWork;
        private readonly IPromotionService _promotionService;
        private readonly IFileUploadService _fileUploadService;
        private readonly StockNotificationService _stockNotificationService;


        public ProductService(
            Core.Interfaces.IUnitOfWork unitOfWork, 
            IPromotionService promotionService, 
            IFileUploadService fileUploadService, 
            StockNotificationService stockNotificationService)
        {
            _unitOfWork = unitOfWork;
            _promotionService = promotionService;
            _fileUploadService = fileUploadService;
            _stockNotificationService = stockNotificationService;
        }

        public async Task<ProductListResult> GetProductsAsync(
            int? categoryId, int? subCategoryId, string? search,
            int page, int pageSize, string? userId)
        {
            // 1. بناء شرط الفلترة
            Expression<Func<Product, bool>> predicate = p =>
                (!categoryId.HasValue || p.ProductCategoryId == categoryId.Value) &&
                (!subCategoryId.HasValue || p.ProductSubCategoryId == subCategoryId.Value) &&
                (string.IsNullOrEmpty(search) || p.Name.Contains(search) || p.Description.Contains(search));

            var matchingProducts = await _unitOfWork.Repository<Product>()
                .FindAsync(predicate, p => p.ProductCategory, p => p.Images);

            var orderedProducts = matchingProducts
                .OrderByDescending(p => p.Id)
                .ToList();

            int totalCount = orderedProducts.Count;

            var pagedProducts = orderedProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 2. بيانات الكارت والـ Notify الخاصة باليوزر (لو مسجل دخول)
            var cartItems = new Dictionary<int, int>();
            var notifyRequestedIds = new HashSet<int>();

            if (!string.IsNullOrEmpty(userId))
            {
                var carts = await _unitOfWork.Repository<Cart>().FindAsync(c => c.UserId == userId, c => c.Items);
                cartItems = carts.SelectMany(c => c.Items).ToDictionary(i => i.ProductId, i => i.Quantity);

                var notifications = await _unitOfWork.Repository<StockNotificationRequest>()
                    .FindAsync(r => r.UserId == userId && !r.IsNotified);
                notifyRequestedIds = notifications.Select(r => r.ProductId).ToHashSet();
            }

            // 3. حساب الـ Promotions لنفس المنتجات دي بس
            var activePromotions = await _promotionService.GetActivePromotionsAsync();
            var productPromotions = BuildPromotionsMap(pagedProducts, activePromotions);

            // 4. تجميع النتيجة النهائية (الـ Mapping بيتم يدوي هنا، مش عن طريق AutoMapper، لأن فيه بيانات مركّبة من مصادر متعددة)
            var productDtos = pagedProducts.Select(p => new ProductListItemDto
            {
                Id = p.Id,
                ProductCategoryId = p.ProductCategoryId,   // 👈 جديد
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                OldPrice = p.OldPrice,
                StockQuantity = p.StockQuantity,
                PictureUrl = GetMainImage(p),
                Category = p.ProductCategory?.Name ?? string.Empty,
                CartQuantity = cartItems.ContainsKey(p.Id) ? cartItems[p.Id] : 0,
                NotifyRequested = notifyRequestedIds.Contains(p.Id),
                BadgeText = productPromotions.ContainsKey(p.Id) ? productPromotions[p.Id].BadgeText : null,
                BadgeColorHex = productPromotions.ContainsKey(p.Id) ? productPromotions[p.Id].BadgeColorHex : null,
                DiscountedPrice = productPromotions.ContainsKey(p.Id) ? productPromotions[p.Id].DiscountedPrice : null
            }).ToList();

            // 5. الكاتيجوريز والـ SubCategories
            var categories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();
            var subCategories = categoryId.HasValue
                ? await _unitOfWork.Repository<ProductSubCategory>().FindAsync(s => s.ProductCategoryId == categoryId.Value)
                : new List<ProductSubCategory>();

            return new ProductListResult
            {
                Products = productDtos,
                TotalCount = totalCount,
                Page = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Categories = categories.Select(c => new CategoryToReturnDto { Id = c.Id, Name = c.Name, ImageUrl = c.ImageUrl }).ToList(),
                SubCategories = subCategories.Select(s => new CategoryToReturnDto { Id = s.Id, Name = s.Name }).ToList()
            };
        }


        public async Task<ProductDetailDto?> GetProductDetailAsync(int id, string? userId)
        {
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdAsync(id, p => p.ProductCategory, p => p.ProductSubCategory, p => p.Images, p => p.Reviews);

            if (product == null) return null;

            var reviews = product.Reviews?.ToList() ?? new List<Review>();
            var oneMonthAgo = DateTime.Now.AddMonths(-1);

            var orderItems = await _unitOfWork.Repository<OrderItem>()
                .FindAsync(oi => oi.ProductId == id
                    && oi.Order.Status != OrderStatus.Cancelled
                    && oi.Order.CreatedAt >= oneMonthAgo);

            bool notifyRequested = false;
            if (!string.IsNullOrEmpty(userId))
            {
                var pending = await _unitOfWork.Repository<StockNotificationRequest>()
                    .FindAsync(r => r.ProductId == id && r.UserId == userId && !r.IsNotified);
                notifyRequested = pending.Any();
            }

            // --- NEW: same promotion calculation Index()/Search() already use,
            // just applied to this single product too. ---
            var activePromotions = await _promotionService.GetActivePromotionsAsync();
            var promoMap = BuildPromotionsMap(new List<Product> { product }, activePromotions);
            promoMap.TryGetValue(product.Id, out var promoInfo);

            return new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OldPrice = product.OldPrice,
                StockQuantity = product.StockQuantity,
                PictureUrl = GetMainImage(product),
                GalleryImageUrls = product.Images.Where(i => !i.IsMain).Select(i => i.ImageUrl).ToList(),
                Category = product.ProductCategory?.Name ?? string.Empty,
                SubCategory = product.ProductSubCategory?.Name ?? string.Empty,
                AvgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                TotalReviews = reviews.Count,
                SoldLastMonth = orderItems.Sum(oi => oi.Quantity),
                NotifyRequested = notifyRequested,
                // --- NEW fields ---
                BadgeText = promoInfo?.BadgeText,
                BadgeColorHex = promoInfo?.BadgeColorHex,
                DiscountedPrice = promoInfo?.DiscountedPrice,
                Reviews = reviews.Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.UserName,
                    Rating = r.Rating,
                    Content = r.Content
                }).ToList()
            };
        }


        public async Task<List<CategoryToReturnDto>> GetSubCategoriesAsync(int categoryId)
        {
            var subCategories = await _unitOfWork.Repository<ProductSubCategory>()
                .FindAsync(s => s.ProductCategoryId == categoryId);

            return subCategories.Select(s => new CategoryToReturnDto { Id = s.Id, Name = s.Name }).ToList();
        }

        public async Task<List<ProductListItemDto>> MapToDtosAsync(List<Product> products, string? userId)
        {
            var cartItems = new Dictionary<int, int>();
            var notifyRequestedIds = new HashSet<int>();

            if (!string.IsNullOrEmpty(userId))
            {
                var carts = await _unitOfWork.Repository<Cart>().FindAsync(c => c.UserId == userId, c => c.Items);
                cartItems = carts.SelectMany(c => c.Items).ToDictionary(i => i.ProductId, i => i.Quantity);

                var notifications = await _unitOfWork.Repository<StockNotificationRequest>()
                    .FindAsync(r => r.UserId == userId && !r.IsNotified);
                notifyRequestedIds = notifications.Select(r => r.ProductId).ToHashSet();
            }
            var activePromotions = await _promotionService.GetActivePromotionsAsync();
            var productPromotions = BuildPromotionsMap(products, activePromotions);

            return products.Select(p => new ProductListItemDto
            {
                Id = p.Id,
                ProductCategoryId = p.ProductCategoryId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                OldPrice = p.OldPrice,
                StockQuantity = p.StockQuantity,
                PictureUrl = GetMainImage(p),
                Category = p.ProductCategory?.Name ?? string.Empty,
                CartQuantity = cartItems.ContainsKey(p.Id) ? cartItems[p.Id] : 0,
                NotifyRequested = notifyRequestedIds.Contains(p.Id),
                BadgeText = productPromotions.ContainsKey(p.Id) ? productPromotions[p.Id].BadgeText : null,
                BadgeColorHex = productPromotions.ContainsKey(p.Id) ? productPromotions[p.Id].BadgeColorHex : null,
                DiscountedPrice = productPromotions.ContainsKey(p.Id) ? productPromotions[p.Id].DiscountedPrice : null
            }).ToList();
        }

        private static string GetMainImage(Product p)
        {
            var mainImage = p.Images?.FirstOrDefault(i => i.IsMain);
            return mainImage?.ImageUrl ?? p.MainImageUrlPath ?? string.Empty;
        }

        private Dictionary<int, ProductPromoInfoLocal> BuildPromotionsMap(List<Product> products, IEnumerable<Promotion> activePromotions)
        {
            var map = new Dictionary<int, ProductPromoInfoLocal>();

            foreach (var product in products)
            {
                var promo = activePromotions
                    .Where(p =>
                        !p.IsFirstPurchaseOnly &&
                        (p.Scope == PromotionScope.AllProducts ||
                         (p.Scope == PromotionScope.Category && p.ProductCategoryId == product.ProductCategoryId) ||
                         (p.Scope == PromotionScope.SubCategory && p.ProductSubCategoryId == product.ProductSubCategoryId) ||
                         (p.Scope == PromotionScope.SpecificProduct && p.ProductId == product.Id)))
                    .OrderByDescending(p => p.Priority)
                    .FirstOrDefault();

                if (promo == null) continue;
                map[product.Id] = new ProductPromoInfoLocal
                {
                    BadgeText = promo.BadgeText,
                    BadgeColorHex = promo.BadgeColorHex,
                    DiscountedPrice = promo.Type == PromotionType.BuyXGetYFree
                        ? null
                        : _promotionService.CalculateDiscountedPrice(product.Price, promo)
                };
            }

            return map;
        }

        private class ProductPromoInfoLocal
        {
            public string? BadgeText { get; set; }
            public string? BadgeColorHex { get; set; }
            public decimal? DiscountedPrice { get; set; }
        }

        public async Task<int> CreateProductAsync(ProductCreateDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                OldPrice = dto.OldPrice,
                StockQuantity = dto.StockQuantity,
                ProductCategoryId = dto.CategoryId,
                ProductSubCategoryId = dto.ProductSubCategoryId
            };

            if (dto.MainImage != null)
            {
                var mainImagePath = await _fileUploadService.UploadImageAsync(
                    dto.MainImage.Content, dto.MainImage.FileName, dto.MainImage.Length, "products");

                product.MainImageUrlPath = mainImagePath;
            }

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();   // عشان نضمن إن product.Id بقى موجود

            if (!string.IsNullOrEmpty(product.MainImageUrlPath))
            {
                await _unitOfWork.Repository<ProductImage>().AddAsync(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = product.MainImageUrlPath,
                    IsMain = true
                });
            }

            if (dto.GalleryImages != null)
            {
                foreach (var image in dto.GalleryImages)
                {
                    if (!_fileUploadService.IsAllowedExtension(image.FileName))
                        continue;

                    var path = await _fileUploadService.UploadImageAsync(
                        image.Content, image.FileName, image.Length, "products");

                    await _unitOfWork.Repository<ProductImage>().AddAsync(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = path,
                        IsMain = false
                    });
                }
            }
            await _unitOfWork.CompleteAsync();

            return product.Id;

        }


        public async Task<bool> UpdateProductAsync(ProductEditDto dto)
        {
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdAsync(dto.Id, p => p.Images);

            if (product == null) return false;

            // 1. تحديث الصورة الرئيسية لو فيه جديدة
            if (dto.NewMainImage != null)
            {
                var oldMainImageRecord = product.Images.FirstOrDefault(img => img.IsMain);
                if (oldMainImageRecord != null)
                {
                    _fileUploadService.DeleteFile(oldMainImageRecord.ImageUrl);
                    _unitOfWork.Repository<ProductImage>().HardDelete(oldMainImageRecord);
                }
                else if (!string.IsNullOrEmpty(product.MainImageUrlPath))
                {
                    _fileUploadService.DeleteFile(product.MainImageUrlPath);
                }

                var newMainImagePath = await _fileUploadService.UploadImageAsync(
                    dto.NewMainImage.Content, dto.NewMainImage.FileName, dto.NewMainImage.Length, "products");

                product.MainImageUrlPath = newMainImagePath;

                await _unitOfWork.Repository<ProductImage>().AddAsync(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = newMainImagePath,
                    IsMain = true
                });
            }   // 👈 الـ if بتاع الصورة الرئيسية بيتقفل هنا بس

            // 2. حذف صور محددة من الـ Gallery (برة الـ if، بتتنفذ دايماً)
            if (dto.DeletedImageIds != null && dto.DeletedImageIds.Any())
            {
                var imagesToDelete = product.Images.Where(img => dto.DeletedImageIds.Contains(img.Id)).ToList();

                foreach (var img in imagesToDelete)
                {
                    _fileUploadService.DeleteFile(img.ImageUrl);
                    _unitOfWork.Repository<ProductImage>().HardDelete(img);
                }
            }

            // 3. إضافة صور جديدة للـ Gallery
            if (dto.NewGalleryImages != null)
            {
                foreach (var image in dto.NewGalleryImages)
                {
                    if (!_fileUploadService.IsAllowedExtension(image.FileName))
                        continue;

                    var path = await _fileUploadService.UploadImageAsync(
                        image.Content, image.FileName, image.Length, "products");

                    await _unitOfWork.Repository<ProductImage>().AddAsync(new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = path,
                        IsMain = false
                    });
                }
            }

            // 4. تحديث باقي بيانات المنتج
            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Description = dto.Description;
            product.ProductCategoryId = dto.ProductCategoryId;
            product.ProductSubCategoryId = dto.ProductSubCategoryId;
            product.OldPrice = dto.OldPrice;
            product.StockQuantity = dto.StockQuantity;
            product.IsFeatured = dto.IsFeatured;
            product.UpdatedAt = DateTime.Now;

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            return true;   // 👈 دلوقتي دايماً هيتنفذ في نهاية الميثود
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return false;

            if (!string.IsNullOrEmpty(product.MainImageUrlPath))
                _fileUploadService.DeleteFile(product.MainImageUrlPath);

            _unitOfWork.Repository<Product>().Delete(product);   // Soft Delete للمنتج نفسه
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<List<CategoryToReturnDto>> GetCategoriesAsync()
        {
            var categories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();
            return categories.Select(c => new CategoryToReturnDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageUrl = c.ImageUrl
            }).ToList();
        }


        public async Task<ProductEditDataDto?> GetProductForEditAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>()
                .GetByIdAsync(id, p => p.Images);

            if (product == null) return null;

            return new ProductEditDataDto
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
                MainImageUrl = product.MainImageUrlPath,
                GalleryImages = product.Images
            .Where(img => !img.IsMain)
            .Select(img => new GalleryImageDto { Id = img.Id, ImageUrl = img.ImageUrl })
            .ToList()
            };
        }

        public async Task<bool> UpdateStockOnlyAsync(int id, int newStockQuantity)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return false;

            int previousStock = product.StockQuantity;
            product.StockQuantity = newStockQuantity;
            product.UpdatedAt = DateTime.Now;

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.CompleteAsync();

            if (previousStock <= 0 && newStockQuantity > 0)
            {
                await _stockNotificationService.NotifySubscribersAsync(product.Id);
            }

            return true;
        }

        public async Task<ReviewResult> AddReviewAsync(ReviewCreateDto dto, string userId, string userName)
        {
            if (dto.ProductId <= 0)
                return new ReviewResult { Success = false, Message = "Invalid Product" };

            if (dto.Rating < 1 || dto.Rating > 5)
                return new ReviewResult { Success = false, Message = "Rating must be between 1 and 5" };

            if (string.IsNullOrWhiteSpace(dto.Content))
                return new ReviewResult { Success = false, Message = "Review content cannot be empty" };

            var existingReviews = await _unitOfWork.Repository<Review>()
                .FindAsync(r => r.ProductId == dto.ProductId && r.UserId == userId);

            if (existingReviews.Any())
                return new ReviewResult { Success = false, Message = "You have already reviewed this product" };

            var newReview = new Review
            {
                ProductId = dto.ProductId,
                Rating = dto.Rating,
                Content = dto.Content,
                CreatedAt = DateTime.Now,
                UserId = userId,
                UserName = userName,
                UserImage = "/images/avatar-01.jpg"
            };

            await _unitOfWork.Repository<Review>().AddAsync(newReview);
            await _unitOfWork.CompleteAsync();

            var allReviews = await _unitOfWork.Repository<Review>().FindAsync(r => r.ProductId == dto.ProductId);
            var avgRating = allReviews.Average(r => r.Rating);

            return new ReviewResult
            {
                Success = true,
                AvgRating = avgRating,
                ReviewsCount = allReviews.Count(),
                Review = new ReviewDto
                {
                    Id = newReview.Id,
                    UserId = newReview.UserId,
                    UserName = newReview.UserName,
                    Rating = newReview.Rating,
                    Content = newReview.Content
                }
            };
        }

        public async Task<ReviewResult> DeleteReviewAsync(int reviewId, string userId, bool isAdmin)
        {
            var review = await _unitOfWork.Repository<Review>().GetByIdAsync(reviewId);

            if (review == null)
                return new ReviewResult { Success = false, Message = "Review not found" };

            if (review.UserId != userId && !isAdmin)
                return new ReviewResult { Success = false, Message = "Forbidden" };

            int productId = review.ProductId;

            _unitOfWork.Repository<Review>().HardDelete(review);
            await _unitOfWork.CompleteAsync();

            var remainingReviews = await _unitOfWork.Repository<Review>().FindAsync(r => r.ProductId == productId);
            var avgRating = remainingReviews.Any() ? remainingReviews.Average(r => r.Rating) : 0;

            return new ReviewResult
            {
                Success = true,
                AvgRating = avgRating,
                ReviewsCount = remainingReviews.Count()
            };
        }

        public async Task<(bool Success, bool RequiresLogin, string Message)> RequestNotifyAsync(int productId, string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return (false, true, "Please login to get notified when this product is back in stock.");

            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null)
                return (false, false, "Product not found.");

            if (product.StockQuantity > 0)
                return (false, false, "This product is already in stock.");

            var existing = await _unitOfWork.Repository<StockNotificationRequest>()
                .FindAsync(r => r.ProductId == productId && r.UserId == userId && !r.IsNotified);

            if (existing.Any())
                return (true, false, "You're already on the notify list for this item.");

            await _unitOfWork.Repository<StockNotificationRequest>().AddAsync(new StockNotificationRequest
            {
                ProductId = productId,
                UserId = userId
            });
            await _unitOfWork.CompleteAsync();

            return (true, false, "We'll email you as soon as it's back in stock!");
        }

        public async Task<List<ProductSearchResultItem>> SearchProductsAsync(string? term, string? userId)
        {
            var products = string.IsNullOrEmpty(term)
                ? await _unitOfWork.Repository<Product>().GetAllAsync(p => p.ProductCategory)
                : await _unitOfWork.Repository<Product>().FindAsync(p => p.Name.Contains(term), p => p.ProductCategory);

            var productsList = products.ToList();

            var cartItems = new Dictionary<int, int>();
            var notifyRequestedIds = new HashSet<int>();

            if (!string.IsNullOrEmpty(userId))
            {
                var carts = await _unitOfWork.Repository<Cart>().FindAsync(c => c.UserId == userId, c => c.Items);
                cartItems = carts.SelectMany(c => c.Items).ToDictionary(i => i.ProductId, i => i.Quantity);

                var notifications = await _unitOfWork.Repository<StockNotificationRequest>()
                    .FindAsync(r => r.UserId == userId && !r.IsNotified);
                notifyRequestedIds = notifications.Select(r => r.ProductId).ToHashSet();
            }

            var activePromotions = await _promotionService.GetActivePromotionsAsync();
            var productPromotions = BuildPromotionsMap(productsList, activePromotions);

            return productsList.Select(p => new ProductSearchResultItem
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                OldPrice = p.OldPrice,
                StockQuantity = p.StockQuantity,
                Description = string.IsNullOrEmpty(p.Description) ? ""
                    : (p.Description.Length > 50 ? p.Description.Substring(0, 50) + "..." : p.Description),
                MainImageUrlPath = GetMainImage(p),
                CategoryName = p.ProductCategory?.Name ?? string.Empty,
                CartQuantity = cartItems.ContainsKey(p.Id) ? cartItems[p.Id] : 0,
                NotifyRequested = notifyRequestedIds.Contains(p.Id),
                BadgeText = productPromotions.ContainsKey(p.Id) ? productPromotions[p.Id].BadgeText : null,
                BadgeColorHex = productPromotions.ContainsKey(p.Id) ? productPromotions[p.Id].BadgeColorHex : null,
                DiscountedPrice = productPromotions.ContainsKey(p.Id) ? productPromotions[p.Id].DiscountedPrice : null
            }).ToList();
        }
    }
}
