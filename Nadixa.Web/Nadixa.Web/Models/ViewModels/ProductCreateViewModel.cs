using Microsoft.AspNetCore.Mvc.Rendering;
using Nadixa.Core.Entities;
using Nadixa.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Web.Models.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
        [Required]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        [Required]
        public int StockQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }


        //public Product Product { get; set; } = new Product();
        public IEnumerable<SelectListItem>? Categories { get; set; }
        public IFormFile? MainImage { get; set; }
        public List<IFormFile>? GalleryImages { get; set; }

        //public bool IsFeatured { get; set; }
        //public bool HasDiscount => Product.OldPrice.HasValue && Product.OldPrice > Product.Price;

        //public List<string> ImageUrls { get; set; } = new List<string>();
        
    }
}
