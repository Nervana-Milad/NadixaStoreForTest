using System.ComponentModel.DataAnnotations;

namespace Nadixa.Web.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="First Name is a Required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is a Required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is a Required")]
        [EmailAddress(ErrorMessage = "Email must be in proper format!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is a Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The Password must match the Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }


        public string? Address { get; set; }
        public string? City { get; set; }
    }
}
