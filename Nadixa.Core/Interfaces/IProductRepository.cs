using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IProductRepository
        {
            Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
                int? categoryId, int? subCategoryId, string? search, int page, int pageSize);
        }
    
}
