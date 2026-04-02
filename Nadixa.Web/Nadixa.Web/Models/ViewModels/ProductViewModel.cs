using Microsoft.AspNetCore.Mvc.Rendering;
using Nadixa.Core.Entities;
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
        public Product Product { get; set; } = new Product();
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IFormFile? MainImage { get; set; }
        public List<IFormFile>? GalleryImages { get; set; }

        public bool IsFeatured { get; set; }
        public bool HasDiscount => Product.OldPrice.HasValue && Product.OldPrice > Product.Price;

        public List<string> ImageUrls { get; set; } = new List<string>();
        
    }
}
