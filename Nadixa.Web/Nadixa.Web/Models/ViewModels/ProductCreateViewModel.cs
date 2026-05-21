using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
        [ValidateNever]
        public List<SelectListItem> Categories { get; set; } = new();

        [Range(1, int.MaxValue, ErrorMessage = "Please select a sub category")]
        public int ProductSubCategoryId { get; set; }
        [ValidateNever]
        public List<SelectListItem> SubCategories { get; set; } = new();
        public IFormFile? MainImage { get; set; }
        public List<IFormFile>? GalleryImages { get; set; }


    }
}
