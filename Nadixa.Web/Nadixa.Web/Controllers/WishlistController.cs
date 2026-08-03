using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;

namespace Nadixa.Web.Controllers
{
    public class WishlistController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IWishlistService _wishlistService;

        public WishlistController(UserManager<AppUser> userManager, IWishlistService wishlistService)
        {
            _userManager = userManager;
            _wishlistService = wishlistService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var items = await _wishlistService.GetWishlistAsync(user.Id);

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _wishlistService.ToggleAsync(user?.Id, productId);

            return Json(new
            {
                success = result.Success,
                requiresLogin = result.RequiresLogin,
                isAdded = result.IsAdded,
                count = result.Count,
                message = result.Message
            });
        }
    }
}