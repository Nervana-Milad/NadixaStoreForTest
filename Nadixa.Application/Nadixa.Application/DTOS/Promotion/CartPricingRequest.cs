using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Promotion
{
    public class CartPricingRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<CartLineItem> Items { get; set; } = new();
        public string? CouponCode { get; set; }
        public decimal BaseShippingFee { get; set; } = 50;

        // خصم نقاط الولاء - عدد النقاط اللي العميل عايز يستبدلها (اختياري)
        public int? RedeemLoyaltyPoints { get; set; }
    }
}
