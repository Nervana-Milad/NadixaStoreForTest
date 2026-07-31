using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nadixa.Core.Entities;

namespace Nadixa.Web.Models.ViewModels
{
    public class ProductEditViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsFeatured { get; set; }
        public int ProductCategoryId { get; set; }
        public int ProductSubCategoryId { get; set; }
        public string? ExistingMainImageUrl { get; set; }

        [ValidateNever]
        public IFormFile? MainImageUrl { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> SubCategories { get; set; }
        [ValidateNever]
        public List<IFormFile> NewGalleryImages { get; set; } = new();
        [ValidateNever]
        public List<ProductImageViewModel> ExistingImages { get; set; } = new();
        public string? DeletedImages { get; set; } = "";
        
    }
}
