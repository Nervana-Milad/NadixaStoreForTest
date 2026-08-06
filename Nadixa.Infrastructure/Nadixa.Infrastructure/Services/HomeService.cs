using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.Promotion;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;

namespace Nadixa.Infrastructure.Services
{
    public class HomeService : IHomeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;
        private readonly IBlogRepository _blogRepository; // لسه بنمطها القديم، مش لمسناها
        private readonly IPromotionService _promotionService;   // 👈 جديد


        public HomeService(IUnitOfWork unitOfWork, IProductService productService, IBlogRepository blogRepository, IPromotionService promotionService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
            _blogRepository = blogRepository;
            _promotionService = promotionService;
        }

        public async Task<HomeIndexResult> GetIndexDataAsync(int? categoryId, string? userId)
        {
            var products = categoryId.HasValue
                ? await _unitOfWork.Repository<Product>()
                    .FindAsync(p => p.ProductCategoryId == categoryId.Value, p => p.ProductCategory, p => p.Images)
                : await _unitOfWork.Repository<Product>()
                    .GetAllAsync(p => p.ProductCategory, p => p.Images);

            var productsList = products.ToList();

            var categories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();

            var orderItems = await _unitOfWork.Repository<OrderItem>()
                .FindAsync(oi => oi.Order.Status != OrderStatus.Cancelled,
                    oi => oi.Product, oi => oi.Product.ProductCategory, oi => oi.Product.Images);

            var bestSellerProducts = orderItems
                .GroupBy(oi => oi.ProductId)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Take(8)
                .Select(g => g.First().Product)
                .ToList();

            var activePromotions = await _promotionService.GetActivePromotionsAsync();

            // 👇 جديد: تحويل Entities لـ DTOs
            var promotionDtos = activePromotions.Select(p => new PromotionDto
            {
                Id = p.Id,
                Name = p.Name,
                BadgeText = p.BadgeText,
                BadgeColorHex = p.BadgeColorHex,
                IsFlashSale = p.IsFlashSale,
                EndDate = p.EndDate,
                Priority = p.Priority,
                ProductCategoryId = p.ProductCategoryId,
                Scope = p.Scope.ToString()
            }).ToList();

            return new HomeIndexResult
            {
                Products = await _productService.MapToDtosAsync(productsList, userId),
                BestSellers = await _productService.MapToDtosAsync(bestSellerProducts, userId),
                Categories = categories.Select(c => new CategoryToReturnDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl
                }).ToList(),
                ActivePromotions = promotionDtos   // 👈 جديد

            };
        }

        public async Task<GlobalSearchResult> GlobalSearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new GlobalSearchResult();

            var products = await _unitOfWork.Repository<Product>()
                .FindAsync(p => p.Name.Contains(term) || p.Description.Contains(term), p => p.ProductCategory);

            var categories = await _unitOfWork.Repository<ProductCategory>()
                .FindAsync(c => c.Name.Contains(term));

            return new GlobalSearchResult
            {
                Products = products.Take(5).Select(p => new ProductSearchItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.MainImageUrlPath,
                    Url = "/Product/Detail/" + p.Id
                }).ToList(),
                Categories = categories.Take(3).Select(c => new CategorySearchItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Url = "/Product/Index?categoryId=" + c.Id
                }).ToList(),
                Blogs = await _blogRepository.SearchAsync(term, 3)
            };
        }
    }
}