using Nadixa.Core.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace Nadixa.Web.Models.ViewModels
{
    public class CouponFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "كود الكوبون مطلوب")]
        [Display(Name = "الكود")]
        public string Code { get; set; } = string.Empty;

        [Required]
        public CouponDiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "قيمة الخصم مطلوبة")]
        public decimal Value { get; set; }

        public decimal? MinOrderAmount { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public int? MaxTotalUsage { get; set; }
        public int? MaxUsagePerUser { get; set; } = 1;
        public bool FirstOrderOnly { get; set; } = false;

        [Required(ErrorMessage = "تاريخ البداية مطلوب")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "تاريخ النهاية مطلوب")]
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(30);

        public bool IsActive { get; set; } = true;
    }
}

