using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class ProductToReturnDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string PictureUrl { get; set; } = string.Empty; // الصورة الرئيسية
        public string Category { get; set; } = string.Empty;   // هنرجع اسم القسم مش رقمه
        public bool IsFeatured { get; set; }
    }
}
