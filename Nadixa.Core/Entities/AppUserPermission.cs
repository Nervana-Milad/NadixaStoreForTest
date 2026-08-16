using Nadixa.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Entities
{
    public class AppUserPermission : BaseEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
