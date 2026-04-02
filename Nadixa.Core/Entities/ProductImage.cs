using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class ProductImage : BaseEntity
    {
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; } // هل دي الصورة الرئيسية اللي بتظهر بره؟

        // ربط بالمنتج
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
