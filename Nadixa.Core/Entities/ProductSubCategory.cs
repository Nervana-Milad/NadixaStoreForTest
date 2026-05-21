using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class ProductSubCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int ProductCategoryId { get; set; }

        public ProductCategory ProductCategory { get; set; }

        // Products
        public ICollection<Product> Products { get; set; }
            = new List<Product>();
    }
}
