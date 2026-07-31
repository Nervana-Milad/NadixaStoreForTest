using Nadixa.Application.DTOS;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class GlobalSearchResult
    {
        public List<ProductSearchItem> Products { get; set; } = new();
        public List<CategorySearchItem> Categories { get; set; } = new();
        public List<BlogSearchItem> Blogs { get; set; } = new();

    }
}
