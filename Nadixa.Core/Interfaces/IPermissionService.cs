using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> UserHasPermissionAsync(ClaimsPrincipal httpUser, string permissionCode);
        Task<List<string>> GetUserPermissionCodesAsync(ClaimsPrincipal httpUser);

    }
}
