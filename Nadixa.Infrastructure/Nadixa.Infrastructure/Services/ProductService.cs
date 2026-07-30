//using Nadixa.Application.Interfaces;
//using Nadixa.Core.DTOS;


//namespace Nadixa.Infrastructure.Services
//{
//    public class ProductService : IProductService
//    {
//        private readonly IProductRepository _productRepository;

//        public ProductService(IProductRepository productRepository)
//        {
//            _productRepository = productRepository;
//        }

//        public async Task<PagedProductsDto> GetPagedAsync(int? categoryId, int? subCategoryId, string? search, int page, int pageSize)
//        {
//            var (items, total) = await _productRepository.GetPagedAsync(categoryId, subCategoryId, search, page, pageSize);

//            return new PagedProductsDto
//            {
//                Page = page,
//                PageSize = pageSize,
//                TotalCount = total,
//                Items = items.Select(p => new ProductListItemDto
//                {
//                    Id = p.Id,
//                    Name = p.Name,
//                    Price = p.Price,
//                    OldPrice = p.OldPrice,
//                    StockQuantity = p.StockQuantity,
//                    Description = p.Description,
//                    MainImageUrlPath = p.MainImageUrlPath,
//                    ProductCategoryId = p.ProductCategoryId,
//                    CategoryName = p.ProductCategory?.Name
//                }).ToList()
//            };
//        }

//        public async Task<ProductDetailDto?> GetDetailAsync(int id)
//        {
//            // NOTE: this calls a new repository method — see README section
//            // "Add this method to IProductRepository / ProductRepository" before wiring this up.
//            var product = await _productRepository.GetByIdWithDetailsAsync(id);
//            if (product == null) return null;

//            var reviews = product.Reviews?.ToList() ?? new List<Nadixa.Core.Entities.Review>();

//            return new ProductDetailDto
//            {
//                Id = product.Id,
//                Name = product.Name,
//                Description = product.Description,
//                Price = product.Price,
//                OldPrice = product.OldPrice,
//                StockQuantity = product.StockQuantity,
//                MainImageUrlPath = product.MainImageUrlPath,
//                CategoryName = product.ProductCategory?.Name,
//                SubCategoryName = product.ProductSubCategory?.Name,
//                GalleryImageUrls = product.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>(),
//                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
//                TotalReviews = reviews.Count
//            };
//        }
//    }
//}
