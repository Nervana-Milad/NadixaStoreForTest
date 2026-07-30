using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class ProductPromoInfo
    {
        public string? BadgeText { get; set; }
        public string? BadgeColorHex { get; set; }
        public decimal? DiscountedPrice { get; set; }
    }
}
