using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Blog
{
    public class BlogListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int CommentsCount { get; set; }
    }
}
