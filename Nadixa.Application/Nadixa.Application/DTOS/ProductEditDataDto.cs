using Nadixa.Application.DTOS;
using System.Collections.Generic;

namespace Nadixa.Application.DTOS
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

