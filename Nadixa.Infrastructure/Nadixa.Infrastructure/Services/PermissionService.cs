using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;


namespace Nadixa.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly NadixaDbContext _context;
        public PermissionService(UserManager<AppUser> userManager, NadixaDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        public async Task<bool> UserHasPermissionAsync(ClaimsPrincipal httpUser, string permissionCode)
        {
            if(httpUser.IsInRole("Admin"))
                return true;

            if (!httpUser.IsInRole("SubAdmin"))
                return false;

            var user = await _userManager.GetUserAsync(httpUser);
            if(user == null)
                return false;

            return await _context.AppUserPermissions
                .Include(p => p.Permission)
                .AnyAsync(p => p.UserId == user.Id && p.Permission.Code == permissionCode);
        }


        public async Task<List<string>> GetUserPermissionCodesAsync(ClaimsPrincipal httpUser)
        {
            var user = await _userManager.GetUserAsync(httpUser);
            if(user == null)
                return new List<string>();

            return await _context.AppUserPermissions
                .Include(p => p.Permission)
                .Where(up => up.UserId == user.Id)
                .Select(up => up.Permission.Code)
                .ToListAsync();
        }
    }
}
