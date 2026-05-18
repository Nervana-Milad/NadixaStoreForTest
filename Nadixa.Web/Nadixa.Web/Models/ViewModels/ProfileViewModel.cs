namespace Nadixa.Web.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public List<OrderViewModel> Orders { get; set; } = new();
    }
}
