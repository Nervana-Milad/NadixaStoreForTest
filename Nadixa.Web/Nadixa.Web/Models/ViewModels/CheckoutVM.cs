using System.ComponentModel.DataAnnotations;

namespace Nadixa.Web.Models.ViewModels
{
    public class CheckoutVM
    {
        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        [MinLength(3)]
        public string FullName { get; set; }

        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must contain exactly 11 digits.")]


        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(100)]
        [MinLength(3)]
        public string Address { get; set; }

        [Required(ErrorMessage = "City is required")]

        public string City { get; set; }

        public string? Notes { get; set; }
       public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
