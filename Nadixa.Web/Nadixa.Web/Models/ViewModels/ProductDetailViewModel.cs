using Nadixa.Core.Entities;

namespace Nadixa.Web.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
