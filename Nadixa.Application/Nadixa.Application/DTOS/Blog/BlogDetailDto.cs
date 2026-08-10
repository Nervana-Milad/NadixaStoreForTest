using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Blog
{
    public class BlogDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Category { get; set; } = string.Empty;
        public int BlogCategoryId { get; set; }
        public DateTime CreateAt { get; set; }
        public List<BlogCommentDto> Comments { get; set; } = new();
    }
}
