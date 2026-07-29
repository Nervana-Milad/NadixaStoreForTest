using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.DTOS
{
    public class ReviewResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public ReviewDto? Review { get; set; }
        public double AvgRating { get; set; }
        public int ReviewsCount { get; set; }
    }
}
