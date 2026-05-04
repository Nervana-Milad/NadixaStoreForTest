using Microsoft.AspNetCore.Mvc.Rendering;
using Nadixa.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Nadixa.Web.Models.ViewModels
{
    public class BlogEditViewModel
    {
        public int Id { get; set; } // مهم جدًا
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }

        // الصورة الحالية (للعرض)
        public string? ExistingImageUrl { get; set; }

        // الصورة الجديدة (اختياري)
        public IFormFile? NewImageFile { get; set; }

        public int BlogCategoryId { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }
    }
}
