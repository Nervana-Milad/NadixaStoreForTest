using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.Promotion;
using System.Collections.Generic;

namespace Nadixa.Web.Models.ViewModels
{
    public class CartIndexViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public CartPricingResult Pricing { get; set; } = new();
        public string? CouponCode { get; set; }
    }
}