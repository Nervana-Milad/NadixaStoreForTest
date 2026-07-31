using Nadixa.Application.DTOS;
using System.ComponentModel.DataAnnotations;

namespace Nadixa.Web.Models.ViewModels
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        [MinLength(3)]
        public string FullName { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must contain exactly 11 digits.")]


        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(100)]
        [MinLength(3)]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]

        public string City { get; set; }

        public string? Notes { get; set; }
       public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }

        //=======================================
        public string? CouponCode { get; set; }              // اللي العميل كتبه/اتخزن من السلة
        public string? CouponError { get; set; }               // لو الكود مرفوض

        public decimal ProductsDiscount { get; set; }          // خصم العروض على المنتجات
        public decimal BundleDiscount { get; set; }             // خصم الباندل
        public decimal CouponDiscount { get; set; }              // خصم الكوبون
        public decimal ShippingDiscount { get; set; }             // خصم الشحن (لو فيه)
        public decimal TotalDiscount =>
            ProductsDiscount + BundleDiscount + CouponDiscount;    // الإجمالي بدون خصم الشحن

        public int LoyaltyPointsToEarn { get; set; }

        public List<AppliedPromotionInfo> AppliedPromotions { get; set; }
            = new List<AppliedPromotionInfo>();
    }
}
