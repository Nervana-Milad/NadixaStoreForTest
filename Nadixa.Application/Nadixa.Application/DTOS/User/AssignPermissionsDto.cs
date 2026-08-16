using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS.User
{
    public class AssignPermissionsDto
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<PermissionCheckboxDto> Permissions { get; set; } = new();
    }
}
