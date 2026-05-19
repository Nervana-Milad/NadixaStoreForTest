using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class ProductCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

       
        public ICollection<ProductSubCategory> SubCategories { get; set; } = new List<ProductSubCategory>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
