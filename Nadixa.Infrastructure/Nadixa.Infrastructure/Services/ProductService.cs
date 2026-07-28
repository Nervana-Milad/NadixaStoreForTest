using Nadixa.Core.DTOS;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPromotionService _promotionService;

        public ProductService(IUnitOfWork unitOfWork, IPromotionService promotionService)
        {
            _unitOfWork = unitOfWork;
            _promotionService = promotionService;
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

        // 👇 جديدة: مشتركة بين GetProductsAsync وHomeService (Best Sellers)
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

        // ===== Helper Methods الخاصة =====

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

    }
        }
