using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Wishlist
{
    public class WishlistToggleResult
    {
        public bool Success { get; set; }
        public bool RequiresLogin { get; set; }
        public bool IsAdded { get; set; }
        public int Count { get; set; }
        public string? Message { get; set; }
    }
}
