using Nadixa.Core.Entities;

namespace Nadixa.Web.Models.ViewModels
{
    public class AdminOrderViewModel
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public DateTime CreatedAt { get; set; }

        public  OrderStatus Status { get; set; }

        public decimal GrandTotal { get; set; }
        public List<OrderStatus> AvailableStatuses { get; set; }
    = new();
    }
}
