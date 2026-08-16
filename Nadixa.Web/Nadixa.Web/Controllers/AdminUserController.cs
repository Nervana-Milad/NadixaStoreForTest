using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.Interfaces;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUserController : Controller
    {
        private readonly IUserManagementService _userManagementService;

        public AdminUserController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManagementService.GetNonAdminUsersAsync();

            var viewModel = users.Select(u => new UserListItemViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                CurrentRole = u.CurrentRole
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            var result = await _userManagementService.ChangeRoleAsync(userId, newRole);

            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ManagePermissions(string userId)
        {
            var dto = await _userManagementService.GetPermissionsForUserAsync(userId);

            if (dto == null)
            {
                TempData["Error"] = "This user must be a Sub-Admin first.";
                return RedirectToAction("Index");
            }

            var vm = new AssignPermissionsViewModel
            {
                UserId = dto.UserId,
                UserName = dto.UserName,
                Permissions = dto.Permissions.Select(p => new PermissionCheckboxViewModel
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    IsChecked = p.IsChecked
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ManagePermissions(string userId, List<int> selectedPermissionIds)
        {
            var result = await _userManagementService.UpdatePermissionsAsync(userId, selectedPermissionIds);

            if (!result.Success)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            return RedirectToAction("Index");
        }
    }
}