using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.DTOS
{
    public class GlobalSearchResult
    {
        public List<ProductSearchItem> Products { get; set; } = new();
        public List<CategorySearchItem> Categories { get; set; } = new();
        public List<BlogSearchItem> Blogs { get; set; } = new();

    }
}
