using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<ProductCategory>> GetAllAsync();
        Task<List<CategorySearchItem>> SearchAsync(string term, int take);

    }
}
