using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Nadixa.Web.Models.ViewModels
{
    public class BlogCreateViewModel
    {
        [Required(ErrorMessage = "Title is Required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 letter")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Short Description is Required")]
        [StringLength(500, ErrorMessage = "Short Description cannot exceed 200 letter")]
        public string ShortDescription { get; set; }

        [Required(ErrorMessage = "Image is Required")]
        public IFormFile? ImageFile { get; set; }

        [Required(ErrorMessage = "Category is Required")]
        public int BlogCategoryId { get; set; } // ✅ مهم جدًا
        
        public IEnumerable<SelectListItem>? Categories { get; set; }

        public DateTime Date { get; set; }
    }
}
