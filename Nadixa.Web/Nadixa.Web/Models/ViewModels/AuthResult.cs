using Nadixa.Application.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class AuthResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public string PictureUrl { get; set; } = string.Empty;
        public List<string> GalleryImageUrls { get; set; } = new();
        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
        public double AvgRating { get; set; }
        public int TotalReviews { get; set; }
        public int SoldLastMonth { get; set; }
        public bool NotifyRequested { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }
}
