using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.order
{
    public class OrderDetailsDto
    {
        public int OrderId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public List<OrderStatus> AvailableStatuses { get; set; } = new();
        public List<OrderItemDto> Items { get; set; } = new();
    }

}
