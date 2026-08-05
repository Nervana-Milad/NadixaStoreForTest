using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Promotion
{
    public class AppliedPromotionInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? BadgeText { get; set; }
        public decimal DiscountAmount { get; set; }
    }
}
