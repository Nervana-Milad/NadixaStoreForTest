namespace Nadixa.Application.DTOS

{
    public class NotifyMeWhenProdRestockDto
    {
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public decimal Price { get; set; }
        public string ProductUrl { get; set; }
    }
}
