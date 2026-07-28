using Nadixa.Core.DTOS;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IProductService
    {
        Task<ProductListResult> GetProductsAsync(
            int? categoryId, int? subCategoryId, string? search,
            int page, int pageSize, string? userId);

        Task<ProductDetailDto?> GetProductDetailAsync(int id, string? userId);

        Task<List<CategoryToReturnDto>> GetSubCategoriesAsync(int categoryId);
        Task<List<ProductListItemDto>> MapToDtosAsync(List<Product> products, string? userId);



    }
}
