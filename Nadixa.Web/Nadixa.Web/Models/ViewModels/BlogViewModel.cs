using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace Nadixa.Web.Models.ViewModels
{
    public class BlogViewModel
    {
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public IFormFile? ImageFile { get; set; }
        public int BlogCategoryId { get; set; } // ✅ مهم جدًا
        public IEnumerable<SelectListItem>? Categories { get; set; }

        public DateTime Date { get; set; }
    }
}
