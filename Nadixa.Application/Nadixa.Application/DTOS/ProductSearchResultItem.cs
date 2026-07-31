using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class ProductSearchResultItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? MainImageUrlPath { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int CartQuantity { get; set; }
        public bool NotifyRequested { get; set; }
        public string? BadgeText { get; set; }
        public string? BadgeColorHex { get; set; }
        public decimal? DiscountedPrice { get; set; }
    }
}
