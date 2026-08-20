using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Blog
{
    public class BlogListResult
    {
        public List<BlogListItemDto> Blogs { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
}
