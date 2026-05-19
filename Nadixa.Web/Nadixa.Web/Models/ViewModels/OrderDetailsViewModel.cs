namespace Nadixa.Web.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; }

        public string FullName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public decimal SubTotal { get; set; }

        public decimal ShippingFee { get; set; } = 50;

        public decimal GrandTotal { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }

        public List<OrderItemViewModel> Items { get; set; } = new();
    }
}
