namespace Nadixa.Web.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

        // Optional: total price for the cart
        public decimal Total => Items.Sum(i => i.TotalPrice);
    }
}
