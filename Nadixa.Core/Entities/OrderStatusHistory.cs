using Nadixa.Core.Common;
using System.ComponentModel.DataAnnotations;

namespace Nadixa.Core.Entities
{
    public class OrderStatusHistory : BaseEntity
    {

        public int OrderId { get; set; }

        public Order Order { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string ChangedBy { get; set; } = string.Empty;
    }
}