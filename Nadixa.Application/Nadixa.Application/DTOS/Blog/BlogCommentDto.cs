using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Blog
{
    public class BlogCommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
