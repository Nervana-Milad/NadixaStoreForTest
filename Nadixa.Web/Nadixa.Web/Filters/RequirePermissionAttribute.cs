using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;

namespace Nadixa.Web.Filters
{
    public class RequirePermissionAttribute : TypeFilterAttribute
    {
        public RequirePermissionAttribute(string permissionCode) : base(typeof(RequirePermissionFilter))
        {
            Arguments = new object[] { permissionCode };
        }
    }

    public class RequirePermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _permissionCode;
        private readonly UserManager<AppUser> _userManager;
        private readonly NadixaDbContext _context;
        public RequirePermissionFilter(string permissionCode, UserManager<AppUser> userManager, NadixaDbContext context)
        {
            _permissionCode = permissionCode;
            _userManager = userManager;
            _context = context;
        }


        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpUser = context.HttpContext.User;

            if(!httpUser.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // الأدمن الأساسي عنده صلاحية كل حاجة تلقائيًا
            if (httpUser.IsInRole("Admin"))
                return;

            // لو مش SubAdmin أصلًا، مرفوض
            if (!httpUser.IsInRole("SubAdmin"))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            var user = await _userManager.GetUserAsync(httpUser);
            if(user == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            bool hasPermission = await _context.AppUserPermissions
                .Include(up => up.Permission)
                .AnyAsync(up => up.UserId == user.Id && up.Permission.Code == _permissionCode);

            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            }
        }
    }
}
