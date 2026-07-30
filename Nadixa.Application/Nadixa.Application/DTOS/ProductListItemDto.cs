using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class ProductListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? Description { get; set; }
        public string? MainImageUrlPath { get; set; }
        public int ProductCategoryId { get; set; }
        public string? CategoryName { get; set; }
    }

}
