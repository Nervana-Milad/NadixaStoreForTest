using Nadixa.Application.DTOS.Product;
using Nadixa.Core.Entities;
using System.Collections.Generic;

namespace Nadixa.Application.DTOS
{
    public class HomeIndexResult
    {
        public List<ProductListItemDto> Products { get; set; } = new();
        public List<ProductListItemDto> BestSellers { get; set; } = new();
        public List<CategoryToReturnDto> Categories { get; set; } = new();
        public List<Nadixa.Core.Entities.Promotion> ActivePromotions { get; set; } = new();   // 👈 جديد

    }
}