using Nadixa.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Web.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }

        public string MainImageUrl { get; set; }

        public bool IsFeatured { get; set; }
        public bool HasDiscount => OldPrice.HasValue && OldPrice > Price;

        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<ColorViewModel> Colors { get; set; } = new List<ColorViewModel>();

    }
}
