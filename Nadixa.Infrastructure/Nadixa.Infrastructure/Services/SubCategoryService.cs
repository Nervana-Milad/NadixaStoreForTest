using AutoMapper;
using Nadixa.Application.DTOS.ProductSubCategory;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileUploadService _fileUploadService;
        private readonly IMapper _mapper;

        public SubCategoryService(IUnitOfWork unitOfWork, IFileUploadService fileUploadService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileUploadService = fileUploadService;
            _mapper = mapper;
        }

        public async Task<List<SubCategoryDto>> GetAllAsync()
        {
            var subCategories = await _unitOfWork.Repository<ProductSubCategory>()
                .GetAllAsync(s => s.ProductCategory);

            return _mapper.Map<List<SubCategoryDto>>(subCategories);
        }

        public async Task<SubCategoryDto?> GetByIdAsync(int id)
        {
            var subCategory = await _unitOfWork.Repository<ProductSubCategory>()
                .GetByIdAsync(id, s => s.ProductCategory);

            if (subCategory == null) return null;

            return _mapper.Map<SubCategoryDto>(subCategory);
        }

        public async Task CreateAsync(SubCategoryCreateDto dto)
        {
            string? imagePath = null;

            if (dto.Image != null)
            {
                imagePath = await _fileUploadService.UploadImageAsync(
                    dto.Image.Content, dto.Image.FileName, dto.Image.Length, "subcategories");
            }
            var subCategory = new ProductSubCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = imagePath,
                ProductCategoryId = dto.ProductCategoryId
            };

            await _unitOfWork.Repository<ProductSubCategory>().AddAsync(subCategory);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> UpdateAsync(SubCategoryEditDto dto)
        {
            var subCategory = await _unitOfWork.Repository<ProductSubCategory>().GetByIdAsync(dto.Id);
            if (subCategory == null) return false;

            if (dto.NewImage != null)
            {
                if (!string.IsNullOrEmpty(subCategory.ImageUrl))
                    _fileUploadService.DeleteFile(subCategory.ImageUrl);

                subCategory.ImageUrl = await _fileUploadService.UploadImageAsync(
                    dto.NewImage.Content, dto.NewImage.FileName, dto.NewImage.Length, "subcategories");
            }
            subCategory.Name = dto.Name;
            subCategory.Description = dto.Description;
            subCategory.ProductCategoryId = dto.ProductCategoryId;

            _unitOfWork.Repository<ProductSubCategory>().Update(subCategory);
            await _unitOfWork.CompleteAsync();

            return true;
        }
        public async Task<SubCategoryDeleteResult> DeleteAsync(int id)
        {
            var subCategory = await _unitOfWork.Repository<ProductSubCategory>().GetByIdAsync(id);
            if (subCategory == null)
                return new SubCategoryDeleteResult { Success = false, ErrorMessage = "SubCategory not found." };

            var hasProducts = await _unitOfWork.Repository<Product>()
    .ExistsAsync(p => p.ProductSubCategoryId == id, includeSoftDeleted: true);


            if (hasProducts)
            {
                return new SubCategoryDeleteResult
                {
                    Success = false,
                    ErrorMessage = "Cannot delete subcategory because it contains products."
                };
            }

            _unitOfWork.Repository<ProductSubCategory>().HardDelete(subCategory);
            await _unitOfWork.CompleteAsync();

            return new SubCategoryDeleteResult { Success = true };
        }

    }
}
