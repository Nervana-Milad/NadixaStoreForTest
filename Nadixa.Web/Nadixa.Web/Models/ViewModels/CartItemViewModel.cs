namespace Nadixa.Web.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; } // Id of the product
        public string ProductName { get; set; } = string.Empty;

        public string MainImageUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Quantity { get; set; } // How many of this product in the cart
        public int StockQuantity { get; set; } // How many of this product in the stock


        // Optional: total price for this item
        public decimal TotalPrice => Price * Quantity;
    }
}
