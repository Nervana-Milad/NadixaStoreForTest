using Nadixa.Application.DTOS.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IUserManagementService
    {
        Task<List<UserListItemDto>> GetNonAdminUsersAsync();
        Task<OperationResult> ChangeRoleAsync(string userId, string newRole);
        Task<AssignPermissionsDto?> GetPermissionsForUserAsync(string userId);
        Task<OperationResult> UpdatePermissionsAsync(string userId, List<int> selectedPermissionIds);
    }
}
