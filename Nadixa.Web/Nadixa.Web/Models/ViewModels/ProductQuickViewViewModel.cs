using Nadixa.Core.Entities;

namespace Nadixa.Web.Models.ViewModels
{
    public class ProductQuickViewViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; } // عشان يظهر مشطوب عليه لو فيه خصم
        public int StockQuantity { get; set; } // الكمية المتاحة


        public string? MainImageUrl { get; set; }

        public List<ProductImage> Images { get; set; } = new();

    }
}
