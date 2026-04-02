namespace Nadixa.Web.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int Id { get; set; }  // CartItem Id
        public int ProductId { get; set; } // Id of the product
        public ProductViewModel Product { get; set; } = null!; // Product details
        public string ProductName { get; set; } = string.Empty;

        public string MainImageUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Quantity { get; set; } // How many of this product in the cart

        // Optional: total price for this item
        public decimal TotalPrice => Product.Price * Quantity;
    }
}
