using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Common;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Helpers;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class WishlistController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public WishlistController(NadixaDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var wishlist = await _context.Wishlists.Include(w => w.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(w => w.UserId == user.Id);

            var viewModel = wishlist?.Items.Where(i => i.Product != null).Select(i => new WishlistItemViewModel
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                Price = i.Product.Price,
                MainImageUrl = i.Product.MainImageUrlPath
            }).ToList() ?? new List<WishlistItemViewModel>();

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult AddToWishlist(int productId)
        {
            var user = _userManager.GetUserId(User);

            if (user == null)
                return Unauthorized(); 

            // add logic
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new
                {
                    success = false,
                    requiresLogin = true,
                    message = AppMessages.LoginRequired
                });
            }

            var user = await _userManager.GetUserAsync(User);

            if(user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var wishlist = await _context.Wishlists.Include(w => w.Items).FirstOrDefaultAsync(w => w.UserId == user.Id);

            if(wishlist == null)
            {
                wishlist = new Wishlist { UserId = user.Id };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            var existingItem = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
            bool isAdded;
            if(existingItem != null)
            {
                _context.WishlistItems.Remove(existingItem);
                isAdded = false;
            }
            else
            {
                wishlist.Items.Add(new WishlistItem{ProductId = productId});
                isAdded = true;
            }

            await _context.SaveChangesAsync();
            var count = wishlist.Items.Count;
            return Json(new
            {
                success = true,
                isAdded,
                count,
                message = isAdded
                    ? AppMessages.WishlistAdded
                    : AppMessages.WishlistRemoved
            });
        }
    }
}
