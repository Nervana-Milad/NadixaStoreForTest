using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class BlogCategory : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Blog> Blogs { get; set; }
    }
}
