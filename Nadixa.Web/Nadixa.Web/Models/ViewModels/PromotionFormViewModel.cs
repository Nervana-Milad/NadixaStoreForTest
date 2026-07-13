using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nadixa.Web.Models.ViewModels
{
    public class PromotionFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العرض مطلوب")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? BadgeText { get; set; }
        public string BadgeColorHex { get; set; } = "#FF3B30";

        [Required]
        public PromotionType Type { get; set; }

        [Required]
        public PromotionScope Scope { get; set; }

        public decimal? Value { get; set; }
        public int? BuyQuantity { get; set; }
        public int? FreeQuantity { get; set; }

        public int? ProductCategoryId { get; set; }
        public int? ProductSubCategoryId { get; set; }
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "تاريخ النهاية مطلوب")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);

        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0;
        public bool IsFirstPurchaseOnly { get; set; } = false;

        // بيانات الدروب داون بتتملى في الكونترولر
        public List<ProductCategory> Categories { get; set; } = new();
        public List<ProductSubCategory> SubCategories { get; set; } = new();
        public List<Product> Products { get; set; } = new();
    }
}
