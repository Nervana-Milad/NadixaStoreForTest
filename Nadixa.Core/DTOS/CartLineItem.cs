using System.Collections.Generic;

namespace Nadixa.Core.DTOs
{
    public class CartLineItem
    {
        public int ProductId { get; set; }
        public int ProductCategoryId { get; set; }
        public int ProductSubCategoryId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }

    public class CartPricingRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<CartLineItem> Items { get; set; } = new();
        public string? CouponCode { get; set; }
        public decimal BaseShippingFee { get; set; } = 50;

        // خصم نقاط الولاء - عدد النقاط اللي العميل عايز يستبدلها (اختياري)
        public int? RedeemLoyaltyPoints { get; set; }
    }

    public class AppliedPromotionInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? BadgeText { get; set; }
        public decimal DiscountAmount { get; set; }
    }

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
