using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS;
using Nadixa.Core.Entities;

namespace Nadixa.Web.Models.ViewModels
{
    public class ProductCardViewModel
    {
        public ProductListItemDto Product { get; set; } = null!;
        public int CartQuantity { get; set; }        // 0 لو مش في الكارت
        public bool NotifyRequested { get; set; }
    }
}
