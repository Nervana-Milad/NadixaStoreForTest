using Nadixa.Application.DTOS;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductListResult> GetProductsAsync(
            int? categoryId, int? subCategoryId, string? search,
            int page, int pageSize, string? userId);

        Task<ProductDetailDto?> GetProductDetailAsync(int id, string? userId);

        Task<List<CategoryToReturnDto>> GetSubCategoriesAsync(int categoryId);
        Task<List<CategoryToReturnDto>> GetCategoriesAsync();
        Task<List<ProductListItemDto>> MapToDtosAsync(List<Product> products, string? userId);
        Task<int> CreateProductAsync(ProductCreateDto dto);
        Task<bool> UpdateProductAsync(ProductEditDto dto);
        Task<bool> DeleteProductAsync(int id);

        Task<ProductEditDataDto?> GetProductForEditAsync(int id); 

        Task<bool> UpdateStockOnlyAsync(int id, int newStockQuantity);

        Task<ReviewResult> AddReviewAsync(ReviewCreateDto dto, string userId, string userName);
        Task<ReviewResult> DeleteReviewAsync(int reviewId, string userId, bool isAdmin);
        Task<(bool Success, bool RequiresLogin, string Message)> RequestNotifyAsync(int productId, string? userId);
        Task<List<ProductSearchResultItem>> SearchProductsAsync(string? term, string? userId);

    }
}
