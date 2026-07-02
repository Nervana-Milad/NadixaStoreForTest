using System.ComponentModel.DataAnnotations;

namespace Nadixa.Core.Entities
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string ChangedBy { get; set; } = string.Empty;
    }
}