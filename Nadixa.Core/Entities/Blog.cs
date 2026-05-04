using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;

        //relationship
        public int BlogCategoryId { get; set; }
        public BlogCategory BlogCategory { get; set; }

        public string? AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
