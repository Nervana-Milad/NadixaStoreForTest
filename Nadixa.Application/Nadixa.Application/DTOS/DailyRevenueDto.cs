using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class DailyRevenueDto
    {
        public string Day { get; set; } = "";   // "Mon", "Tue" ...
        public decimal Revenue { get; set; }
    }
}
