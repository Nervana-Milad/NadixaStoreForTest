using Nadixa.Core.Entities;

namespace Nadixa.Web.Models.ViewModels
{
    public class ProductCardViewModel
    {
        public Product Product { get; set; }
        public int CartQuantity { get; set; }        // 0 لو مش في الكارت
        public bool NotifyRequested { get; set; }
    }
}
