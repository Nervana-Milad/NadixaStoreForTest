using Nadixa.Application.DTOS.Review;
using System;
using System.Collections.Generic;

namespace Nadixa.Application.DTOS.Product
{
    // Returned by GET /api/products/{id}
    public class ProductDetailDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        public int StockQuantity { get; set; }

        public string? PictureUrl { get; set; }

        public List<string> GalleryImageUrls { get; set; } = new();

        public string Category { get; set; } = string.Empty;

        public string SubCategory { get; set; } = string.Empty;

        public double AvgRating { get; set; }

        public int TotalReviews { get; set; }

        public int SoldLastMonth { get; set; }

        public bool NotifyRequested { get; set; }

        public List<ReviewDto> Reviews { get; set; } = new();

        public string? BadgeText { get; set; }
        public string? BadgeColorHex { get; set; }
        public decimal? DiscountedPrice { get; set; }

    }
}