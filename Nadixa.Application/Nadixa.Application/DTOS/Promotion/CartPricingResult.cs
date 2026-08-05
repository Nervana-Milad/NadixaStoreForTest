using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Promotion
{
    public class CartPricingResult
    {
        public decimal SubTotal { get; set; }
        public decimal ProductsDiscountTotal { get; set; }
        public decimal BundleDiscountTotal { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal LoyaltyDiscount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal ShippingDiscount { get; set; }
        public decimal GrandTotal { get; set; }
        public int LoyaltyPointsToEarn { get; set; }
        public List<AppliedPromotionInfo> AppliedPromotions { get; set; } = new();
        public string? CouponError { get; set; }
    }
}
