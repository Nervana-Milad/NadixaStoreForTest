namespace Nadixa.Web.Models.ViewModels
{
    public class AdminOrderViewModel
    {
        public int Id { get; set; }

        public string CustomerName { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }

        public decimal GrandTotal { get; set; }
    }
}
