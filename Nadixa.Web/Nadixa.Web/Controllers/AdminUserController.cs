using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;
using Nadixa.Core.Common;

namespace Nadixa.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly NadixaDbContext _context;
        public AdminUserController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            NadixaDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin")) continue;

                result.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email ?? "",
                    CurrentRole = roles.FirstOrDefault() ?? "User"
                });
            }
            return View(result);
        }


        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            if (newRole != "User" && newRole != "SubAdmin")
            {
                TempData["Error"] = "Invalid role.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            if (!await _roleManager.RoleExistsAsync("SubAdmin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("SubAdmin"));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains("Admin"))
            {
                TempData["Error"] = "Cannot change role of Admin.";
                return RedirectToAction("Index");
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            if (newRole == "User")
            {
                // Remove all permissions for this user
                var userPermissions = _context.AppUserPermissions.Where(up => up.UserId == user.Id);
                _context.AppUserPermissions.RemoveRange(userPermissions);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Role updated to {newRole}.";
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> ManagePermissions(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("SubAdmin"))
            {
                TempData["Error"] = "This user must be a Sub-Admin first.";
                return RedirectToAction("Index");
            }

            var allPermissions = await _context.Permissions.OrderBy(p => p.Name).ToListAsync();
            var userPermissionIds = await _context.AppUserPermissions
                .Where(up => up.UserId == userId)
                .Select(up => up.PermissionId)
                .ToListAsync();

            var vm = new AssignPermissionsViewModel
            {
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}".Trim(),
                Permissions = allPermissions.Select(p => new PermissionCheckboxViewModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    IsChecked = userPermissionIds.Contains(p.Id)
                }).ToList()
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> ManagePermissions(string userId, List<int> selectedPermissionIds)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var existing = _context.AppUserPermissions.Where(up => up.UserId == userId);
            _context.AppUserPermissions.RemoveRange(existing);

            selectedPermissionIds ??= new List<int>();

            foreach (var permId in selectedPermissionIds)
            {
                _context.AppUserPermissions.Add(new AppUserPermission
                {
                    UserId = userId,
                    PermissionId = permId
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Permissions updated successfully.";
            return RedirectToAction("Index");
        }


    }        
}
