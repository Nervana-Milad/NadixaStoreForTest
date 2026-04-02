using System.ComponentModel.DataAnnotations;

namespace Nadixa.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is a Required")]
        [EmailAddress(ErrorMessage = "Email must be in proper format!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is a Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
