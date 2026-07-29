using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.DTOS
{
    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = "";
        public decimal Total { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
