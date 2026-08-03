using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class CartActionResult
    {
        public bool Success { get; set; }
        public bool RequiresLogin { get; set; }
        public string? Message { get; set; }
        public int CartCount { get; set; }
        public int Quantity { get; set; }
    }
}
