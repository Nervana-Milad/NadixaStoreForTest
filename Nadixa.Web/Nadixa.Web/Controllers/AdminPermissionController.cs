//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Nadixa.Infrastructure.Data;
//using Nadixa.Core.Common;

//namespace Nadixa.Web.Controllers
//{
//    [Authorize(Roles = "Admin")]
//    public class AdminPermissionController : Controller
//    {
//        private readonly NadixaDbContext _context;
//        public AdminPermissionController(NadixaDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var permissions = await _context.Permissions.OrderBy(p => p.Name).ToListAsync();

//            return View(permissions);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(string code, string name, string? description)
//        {

//            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
//            {
//                TempData["Error"] = AppMessages.CodeAndNameRequired;
//                return RedirectToAction("Index");
//            }

//            bool exists = await _context.Permissions.AnyAsync(p => p.Code == code);
//            if (exists)
//            {
//                TempData["Error"] = AppMessages.PermissionCodeExists;
//                return RedirectToAction("Index");
//            }

//            _context.Permissions.Add(new Nadixa.Core.Entities.Permission
//            {
//                Code = code.Trim(),
//                Name = name.Trim(),
//                Description = description
//            });

//            await _context.SaveChangesAsync();

//            TempData["Success"] = AppMessages.PermissionCreated;
//            return RedirectToAction("Index");
//        }

//        [HttpPost]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var permission = await _context.Permissions.FindAsync(id);
//            if (permission != null)
//            {
//                _context.Permissions.Remove(permission);
//                await _context.SaveChangesAsync();

//                TempData["Success"] = AppMessages.PermissionDeleted;
//            }

//            return RedirectToAction("Index");
//        }

//    }
//}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.DTOS.Permission;
using Nadixa.Application.Interfaces;

namespace Nadixa.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPermissionController : Controller
    {
        private readonly IPermissionManagementService _permissionManagementService;

        public AdminPermissionController(IPermissionManagementService permissionManagementService)
        {
            _permissionManagementService = permissionManagementService;
        }

        public async Task<IActionResult> Index()
        {
            var permissions = await _permissionManagementService.GetAllAsync();
            return View(permissions);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string code, string name, string? description)
        {
            var dto = new PermissionCreateDto
            {
                Code = code,
                Name = name,
                Description = description
            };

            var result = await _permissionManagementService.CreateAsync(dto);

            TempData[result.Success ? "Success" : "Error"] = result.Message;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _permissionManagementService.DeleteAsync(id);

            if (result.Success)
                TempData["Success"] = result.Message;

            return RedirectToAction("Index");
        }
    }
}