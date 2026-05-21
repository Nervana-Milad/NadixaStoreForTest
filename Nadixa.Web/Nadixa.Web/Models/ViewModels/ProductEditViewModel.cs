using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nadixa.Core.Entities;

namespace Nadixa.Web.Models.ViewModels
{
    public class ProductEditViewModel
    {
        public Product Product { get; set; } = new Product();
        [ValidateNever]
        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> SubCategories { get; set; }

        [ValidateNever]
        public IFormFile MainImageUrl { get; set; }

        public bool IsFeatured { get; set; }
        public bool HasDiscount => Product.OldPrice.HasValue && Product.OldPrice > Product.Price;

        //public List<string> ImageUrls { get; set; } = new List<string>();

        public List<ProductImageViewModel> ExistingImages { get; set; } = new List<ProductImageViewModel>();

        [ValidateNever]
        public List<IFormFile> NewGalleryImages { get; set; } = new List<IFormFile>();

        public string? DeletedImages { get; set; } = "";  // comma-separated IDs

    }
}
