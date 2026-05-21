using System.ComponentModel.DataAnnotations;

namespace Nadixa.Web.Models.ViewModels
{
    public class ProductSubCategoryViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public IFormFile? ImageFile { get; set; }

        // Parent Category
        [Required]
        public int ProductCategoryId { get; set; }

        // For Dropdown
        public List<ProductCategoryViewModel>? Categories { get; set; }
    }
}
