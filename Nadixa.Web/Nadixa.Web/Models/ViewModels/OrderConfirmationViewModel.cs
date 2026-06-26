namespace Nadixa.Web.Models.ViewModels
{
    public class OrderConfirmationViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string? Notes { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public List<OrderConfirmationItem> Items { get; set; } = new();
    }

    public class OrderConfirmationItem
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal => Price * Quantity;
    }
}
