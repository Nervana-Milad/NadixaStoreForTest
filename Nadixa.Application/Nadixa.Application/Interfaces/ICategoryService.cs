using Nadixa.Application.DTOS.ProductCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
        Task<CategoryDto?> GetByIdAsync(int id);
        Task CreateAsync(CategoryCreateDto dto);
        Task<bool> UpdateAsync(CategoryEditDto dto);
        Task<CategoryDeleteResult> DeleteAsync(int id);
    }
}
