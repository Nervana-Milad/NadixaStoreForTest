using Nadixa.Application.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class HomeApiResult
    {
        public List<ProductToReturnDto> Products { get; set; } = new();
        public List<CategoryToReturnDto> Categories { get; set; } = new();
        public List<ProductToReturnDto> BestSellers { get; set; } = new();
        public Dictionary<int, ProductPromoInfo> ProductPromotions { get; set; } = new();


    }
}
