using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Wishlist
{
    public class WishlistItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? MainImageUrl { get; set; }
        public int StockQuantity { get; set; }
        public int CartQuantity { get; set; }
    }
}
