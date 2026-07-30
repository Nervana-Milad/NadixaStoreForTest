using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    // Returned by GET /api/products/{id}
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? MainImageUrlPath { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public List<string> GalleryImageUrls { get; set; } = new();
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}
