using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class BlogComment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        //Blog Relation
        public int BlogId { get; set; }
        public Blog Blog { get; set; }


        //User Relation
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }


    }
}
