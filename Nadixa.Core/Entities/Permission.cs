using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class Permission : BaseEntity
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;   // زي "EditProductStatus"
        public string Name { get; set; } = string.Empty;   // زي "Edit Product Status"
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<AppUserPermission> UserPermissions { get; set; } = new List<AppUserPermission>();

    }
}
