namespace Nadixa.Web.Models.ViewModels
{
    public class WishlistItemViewModel
    {
        public int ProductId { get; set; } // Id of the product
        public string ProductName { get; set; } = string.Empty;

        public string MainImageUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
}
