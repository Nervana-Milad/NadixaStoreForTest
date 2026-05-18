using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Models.ViewModels
{
    public class CartMiniViewModel
    {
        //public List<CartItemViewModel> Items { get; set; }
        public List<CartItemViewModel> Items { get; set; }
       = new List<CartItemViewModel>();
        public int CartCount => Items.Sum(x => x.Quantity);
        public decimal Total => Items.Sum(i => i.TotalPrice);
    }
}
