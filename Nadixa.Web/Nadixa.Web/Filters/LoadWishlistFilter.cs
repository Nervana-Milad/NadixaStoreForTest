using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Web.Filters
{
    public class LoadWishlistFilter : IAsyncActionFilter
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public LoadWishlistFilter(NadixaDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);
            List<int> wishlistIds = new List<int>();

            if (user != null)
            {
                wishlistIds = await _context.WishlistItems.Where(w => w.Wishlist.UserId == user.Id).Select(w => w.ProductId).ToListAsync();
            }

            if(context.Controller is Controller controller)
            {
                controller.ViewBag.WishlistIds = wishlistIds;
            }
            await next();
        }
    }
}
