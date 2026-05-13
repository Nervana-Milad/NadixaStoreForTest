namespace Nadixa.Web.Models.ViewModels
{
    public class CheckoutVM
    {
        public string FullName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string? Notes { get; set; }



        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
