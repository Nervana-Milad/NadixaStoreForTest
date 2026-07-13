using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class StockNotificationRequest
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public bool IsNotified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
