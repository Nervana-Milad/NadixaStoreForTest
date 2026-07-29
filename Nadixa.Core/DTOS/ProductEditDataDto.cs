using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.DTOS
{
    public class ProductEditDataDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsFeatured { get; set; }
        public int ProductCategoryId { get; set; }
        public int ProductSubCategoryId { get; set; }
        public string? MainImageUrl { get; set; }
        public List<GalleryImageDto> GalleryImages { get; set; } = new();

    }
}
