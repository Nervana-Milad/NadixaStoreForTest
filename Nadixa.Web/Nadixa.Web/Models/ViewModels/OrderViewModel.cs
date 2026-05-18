namespace Nadixa.Web.Models.ViewModels
{
    public class OrderViewModel
    {
        //public string FullName { get; set; }
        //public string Address { get; set; }
        //public string Phone { get; set; }


        //public decimal SubTotal { get; set; }
        //public decimal ShippingFee { get; set; }
        //public decimal GrandTotal { get; set; }


        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Status { get; set; }

        public decimal GrandTotal { get; set; }
    }
}
