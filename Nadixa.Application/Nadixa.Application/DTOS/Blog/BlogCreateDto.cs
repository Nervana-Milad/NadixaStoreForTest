using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Blog
{
    public class BlogCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int BlogCategoryId { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public FileUploadRequest? Image { get; set; }
    }
}
