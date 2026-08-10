using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class Blog : BaseEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;

        //BlogCategory Relation
        public int BlogCategoryId { get; set; }
        public BlogCategory BlogCategory { get; set; }

        //User Relation
        public string? AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        //Comments Relation
        public ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
    }
}
