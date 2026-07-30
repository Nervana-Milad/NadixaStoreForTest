using Nadixa.Application.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IProductService
    {
        Task<PagedProductsDto> GetPagedAsync(int? categoryId, int? subCategoryId, string? search, int page, int pageSize);
        Task<ProductDetailDto?> GetDetailAsync(int id);
    }   
}
