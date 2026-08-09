using AutoMapper;
using Nadixa.Application.DTOS.ProductCategory;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IFileUploadService fileUploadService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileUploadService = fileUploadService;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _unitOfWork.Repository<ProductCategory>().GetAllAsync();
            var ordered = categories.OrderBy(c => c.Name).ToList();

            return _mapper.Map<List<CategoryDto>>(ordered);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.Repository<ProductCategory>().GetByIdAsync(id);
            if (category == null) return null;

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task CreateAsync(CategoryCreateDto dto)
        {
            string? imagePath = null;

            if (dto.Image != null)
            {
                imagePath = await _fileUploadService.UploadImageAsync(
                    dto.Image.Content, dto.Image.FileName, dto.Image.Length, "categories");
            }
            var category = new ProductCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = imagePath
            };

            await _unitOfWork.Repository<ProductCategory>().AddAsync(category);
            await _unitOfWork.CompleteAsync();
        }
        public async Task<bool> UpdateAsync(CategoryEditDto dto)
        {
            var category = await _unitOfWork.Repository<ProductCategory>().GetByIdAsync(dto.Id);
            if (category == null) return false;

            if (dto.NewImage != null)
            {
                // امسحي الصورة القديمة من على الديسك لو موجودة
                if (!string.IsNullOrEmpty(category.ImageUrl))
                    _fileUploadService.DeleteFile(category.ImageUrl);

                category.ImageUrl = await _fileUploadService.UploadImageAsync(
                    dto.NewImage.Content, dto.NewImage.FileName, dto.NewImage.Length, "categories");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;

            _unitOfWork.Repository<ProductCategory>().Update(category);
            await _unitOfWork.CompleteAsync();

            return true;
        }


        public async Task<CategoryDeleteResult> DeleteAsync(int id)
        {
            var category = await _unitOfWork.Repository<ProductCategory>().GetByIdAsync(id);
            if (category == null)
                return new CategoryDeleteResult { Success = false, ErrorMessage = "Category not found." };

            var hasProducts = await _unitOfWork.Repository<Product>()
                .ExistsAsync(p => p.ProductCategoryId == id);

            if (hasProducts)
            {
                return new CategoryDeleteResult
                {
                    Success = false,
                    ErrorMessage = $"Cannot delete '{category.Name}' because it has product(s). Please delete the products first."
                };
            }

            _unitOfWork.Repository<ProductCategory>().Delete(category);
            await _unitOfWork.CompleteAsync();

            return new CategoryDeleteResult { Success = true };
        }
    }
}
