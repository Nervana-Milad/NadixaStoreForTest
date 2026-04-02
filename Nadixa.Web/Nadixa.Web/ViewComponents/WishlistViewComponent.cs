using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.ViewComponents
{
    public class WishlistViewComponent : ViewComponent
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManger;
        public WishlistViewComponent(NadixaDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManger = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManger.GetUserAsync(HttpContext.User);
            int count = 0;
            if(user != null)
            {
                var wishlist = await _context.Wishlists.Include(w => w.Items).FirstOrDefaultAsync(w => w.UserId == user.Id);

                count = wishlist?.Items.Count ?? 0;
            }
            return View(new WishlistMiniViewModel
            {
                Count = count
            });
        }
    }
}
