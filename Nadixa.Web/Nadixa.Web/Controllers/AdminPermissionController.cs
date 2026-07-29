using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Infrastructure.Data;
using Nadixa.Core.Common;

namespace Nadixa.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPermissionController : Controller
    {
        private readonly NadixaDbContext _context;
        public AdminPermissionController(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var permissions = await _context.Permissions.OrderBy(p => p.Name).ToListAsync();

            return View(permissions);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string code, string name, string? description)
        {

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = AppMessages.CodeAndNameRequired;
                return RedirectToAction("Index");
            }

            bool exists = await _context.Permissions.AnyAsync(p => p.Code == code);
            if (exists)
            {
                TempData["Error"] = AppMessages.PermissionCodeExists;
                return RedirectToAction("Index");
            }

            _context.Permissions.Add(new Nadixa.Core.Entities.Permission
            {
                Code = code.Trim(),
                Name = name.Trim(),
                Description = description
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = AppMessages.PermissionCreated;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission != null)
            {
                _context.Permissions.Remove(permission);
                await _context.SaveChangesAsync();

                TempData["Success"] = AppMessages.PermissionDeleted;
            }

            return RedirectToAction("Index");
        }
    
    }
}
