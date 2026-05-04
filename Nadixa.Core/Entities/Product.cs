using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; } // عشان يظهر مشطوب عليه لو فيه خصم
        public int StockQuantity { get; set; } // الكمية المتاحة
        public bool IsFeatured { get; set; } // هل يظهر في الصفحة الرئيسية؟

        // Foreign Key (ربط المنتج بالقسم)
        public int CategoryId { get; set; }
        public ProductCategory? Category { get; set; }

        public string? MainImageUrlPath { get; set; }

        // علاقات (صور وألوان)
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductColor> Colors { get; set; } = new List<ProductColor>();

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();




    }
}
