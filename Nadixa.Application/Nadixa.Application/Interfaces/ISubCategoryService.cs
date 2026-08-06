using Nadixa.Application.DTOS.ProductSubCategory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface ISubCategoryService
    {
        Task<List<SubCategoryDto>> GetAllAsync();
        Task<SubCategoryDto?> GetByIdAsync(int id);
        Task CreateAsync(SubCategoryCreateDto dto);
        Task<bool> UpdateAsync(SubCategoryEditDto dto);
        Task<SubCategoryDeleteResult> DeleteAsync(int id);
    }
}
