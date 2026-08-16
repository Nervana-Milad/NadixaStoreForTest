using Microsoft.AspNetCore.Identity;
using Nadixa.Application.DTOS.User;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserManagementService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<UserListItemDto>> GetNonAdminUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserListItemDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Admin")) continue;

                result.Add(new UserListItemDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    CurrentRole = roles.FirstOrDefault() ?? "No Role"
                });
            }
            return result;
        }

        public async Task<OperationResult> ChangeRoleAsync(string userId, string newRole)
        {
            if (newRole != "User" && newRole != "SubAdmin")
                return new OperationResult { Success = false, Message = "Invalid role." };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new OperationResult { Success = false, Message = "User not found." };

            if (!await _roleManager.RoleExistsAsync("SubAdmin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("SubAdmin"));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains("Admin"))
                return new OperationResult { Success = false, Message = "Cannot change role of Admin." };

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            if (newRole == "User")
            {
                var userPermissions = await _unitOfWork.Repository<AppUserPermission>()
                    .FindAsync(up => up.UserId == user.Id);

                foreach (var perm in userPermissions)
                {
                    _unitOfWork.Repository<AppUserPermission>().HardDelete(perm);
                }

                await _unitOfWork.CompleteAsync();
            }

            return new OperationResult { Success = true, Message = $"Role updated to {newRole}." };

        }

        public async Task<AssignPermissionsDto?> GetPermissionsForUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("SubAdmin"))
                return null;

            var allPermissions = await _unitOfWork.Repository<Permission>().GetAllAsync();
            var orderedPermissions = allPermissions.OrderBy(p => p.Name).ToList();

            var userPermissions = await _unitOfWork.Repository<AppUserPermission>()
                .FindAsync(up => up.UserId == userId);

            var userPermissionIds = userPermissions.Select(up => up.PermissionId).ToList();


            return new AssignPermissionsDto
            {
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}".Trim(),
                Permissions = orderedPermissions.Select(p => new PermissionCheckboxDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    IsChecked = userPermissionIds.Contains(p.Id)
                }).ToList()
            };
        }

        public async Task<OperationResult> UpdatePermissionsAsync(string userId, List<int> selectedPermissionIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new OperationResult { Success = false, Message = "User not found." };

            var existing = await _unitOfWork.Repository<AppUserPermission>()
                .FindAsync(up => up.UserId == userId);

            foreach (var perm in existing)
            {
                _unitOfWork.Repository<AppUserPermission>().HardDelete(perm);
            }
            selectedPermissionIds ??= new List<int>();

            foreach (var permId in selectedPermissionIds)
            {
                await _unitOfWork.Repository<AppUserPermission>().AddAsync(new AppUserPermission
                {
                    UserId = userId,
                    PermissionId = permId
                });
            }

            await _unitOfWork.CompleteAsync();

            return new OperationResult { Success = true, Message = "Permissions updated successfully." };

        }

    }
}
