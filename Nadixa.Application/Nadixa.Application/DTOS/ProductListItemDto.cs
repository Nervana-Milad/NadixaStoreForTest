using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class ProductListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
        public int ProductCategoryId { get; set; }
        public string Category { get; set; } = string.Empty;
        public int CartQuantity { get; set; }
        public bool NotifyRequested { get; set; }
        public string? BadgeText { get; set; }
        public string? BadgeColorHex { get; set; }
        public decimal? DiscountedPrice { get; set; }
    }

    public class ProductListResult
    {
        public List<ProductListItemDto> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public List<CategoryToReturnDto> Categories { get; set; } = new();
        public List<CategoryToReturnDto> SubCategories { get; set; } = new();
    }
}
