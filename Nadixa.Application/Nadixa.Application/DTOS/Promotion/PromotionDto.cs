using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Promotion
{
    public class PromotionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? BadgeText { get; set; }
        public string? BadgeColorHex { get; set; }
        public bool IsFlashSale { get; set; }
        public DateTime EndDate { get; set; }
        public int Priority { get; set; }
        public int? ProductCategoryId { get; set; }
        public string Scope { get; set; } = string.Empty;
    }
}
