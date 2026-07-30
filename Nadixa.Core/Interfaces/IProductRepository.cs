using Nadixa.Core.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync(int? categoryId);
        Task<List<Product>> GetBestSellersAsync(int count);
        Task<List<ProductSearchItem>> SearchAsync(string term, int take);
        Task<Product?> GetByIdWithDetailsAsync(int id);
        Task<(List<Product> Items, int TotalCount)> GetPagedAsync(int? categoryId, int? subCategoryId, string? search, int page,int pageSize);

    }
}
