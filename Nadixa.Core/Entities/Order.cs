using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Processing,
        Shipped,
        OutForDelivery,
        Delivered,
        Cancelled
    }
    public class Order : BaseEntity
    {


        public string UserId { get; set; }

        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string? Notes { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }

        public ICollection<OrderStatusHistory> StatusHistory { get; set; }
    = new List<OrderStatusHistory>();
    }
}
