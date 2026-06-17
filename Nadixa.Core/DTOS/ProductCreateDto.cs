using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class ProductCreateDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        // هنستقبل الصورة كرابط مؤقتاً لحد ما نعمل رفع الملفات
        public string MainImageUrl { get; set; } = string.Empty;
    }
}
