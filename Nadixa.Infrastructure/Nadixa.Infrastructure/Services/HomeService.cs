using Nadixa.Core.DTOS;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class HomeService : IHomeService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IStockNotificationRepository _stockNotificationRepository;
        private readonly IBlogRepository _blogRepository;
        private readonly IPromotionService _promotionService;
        public HomeService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ICartRepository cartRepository,
            IStockNotificationRepository stockNotificationRepository,
            IBlogRepository blogRepository,
            IPromotionService promotionService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _cartRepository = cartRepository;
            _stockNotificationRepository = stockNotificationRepository;
            _blogRepository = blogRepository;
            _promotionService = promotionService;
        }

        public async Task<HomeIndexResult> GetIndexDataAsync(int? categoryId, string? userId)
        {
            var result = new HomeIndexResult();

            result.Products = await _productRepository.GetAllAsync(categoryId);
            result.Categories = await _categoryRepository.GetAllAsync();
            result.BestSellers = await _productRepository.GetBestSellersAsync(8);

            if (!string.IsNullOrEmpty(userId))
            {
                result.CartItems = await _cartRepository.GetCartItemsAsync(userId);
                result.NotifyRequestedProductIds = await _stockNotificationRepository.GetPendingProductIdsAsync(userId);
            }
            result.ProductPromotions = await BuildProductPromotionsAsync(result.Products, result.BestSellers);

            return result;
        }

        public async Task<GlobalSearchResult> GlobalSearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new GlobalSearchResult();

            return new GlobalSearchResult
            {
                Products = await _productRepository.SearchAsync(term, 5),
                Categories = await _categoryRepository.SearchAsync(term, 3),
                Blogs = await _blogRepository.SearchAsync(term, 3)
            };
        }

        private async Task<Dictionary<int, ProductPromoInfo>> BuildProductPromotionsAsync(
            List<Product> products, List<Product> bestSellers)
        {
            var activePromotions = await _promotionService.GetActivePromotionsAsync();

            var productsForPromoCheck = products
                .Concat(bestSellers)
                .GroupBy(p => p.Id)
                .Select(g => g.First());

            var productPromotions = new Dictionary<int, ProductPromoInfo>();
            foreach (var product in productsForPromoCheck)
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
                productPromotions[product.Id] = new ProductPromoInfo
                {
                    BadgeText = promo.BadgeText,
                    BadgeColorHex = promo.BadgeColorHex,
                    DiscountedPrice = promo.Type == PromotionType.BuyXGetYFree
                        ? null
                        : _promotionService.CalculateDiscountedPrice(product.Price, promo)
                };
            }

            return productPromotions;
        }


    }
        }
