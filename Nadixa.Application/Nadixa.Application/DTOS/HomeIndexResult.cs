using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class HomeIndexResult
    {
        public List<Product> Products { get; set; } = new();
        public List<ProductCategory> Categories { get; set; } = new();
        public Dictionary<int, int> CartItems { get; set; } = new();
        public HashSet<int> NotifyRequestedProductIds { get; set; } = new();
        public List<Product> BestSellers { get; set; } = new();
        public Dictionary<int, ProductPromoInfo> ProductPromotions { get; set; } = new();

    }
}
