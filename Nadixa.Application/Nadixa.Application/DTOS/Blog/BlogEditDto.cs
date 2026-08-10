using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.Blog
{
    public class BlogEditDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int BlogCategoryId { get; set; }
        public FileUploadRequest? NewImage { get; set; }
    }
}
