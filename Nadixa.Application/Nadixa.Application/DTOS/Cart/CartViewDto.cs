using Nadixa.Application.DTOS.Promotion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Cart
{
    public class CartViewDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public CartPricingResult Pricing { get; set; } = new();
        public string? CouponCode { get; set; }
    }
}
