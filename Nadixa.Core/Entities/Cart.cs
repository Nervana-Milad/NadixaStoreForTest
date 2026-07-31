using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class Cart : BaseEntity
    {

        public int Id { get; set; }
        public string UserId { get; set;}
        public List<CartItem> Items { get; set; } = new List<CartItem>();


    }
}
