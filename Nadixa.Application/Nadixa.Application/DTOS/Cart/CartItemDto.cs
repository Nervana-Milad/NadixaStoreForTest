using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Cart
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public string? MainImageUrl { get; set; }
        public string? PromoBadgeText { get; set; }
        public string? PromoBadgeColorHex { get; set; }
        public decimal? DiscountedUnitPrice { get; set; }
    }
}
