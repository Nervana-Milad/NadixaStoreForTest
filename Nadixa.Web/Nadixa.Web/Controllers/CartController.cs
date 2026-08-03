using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Core.Entities;
using Nadixa.Web.Helpers;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICartService _cartService;

        public CartController(UserManager<AppUser> userManager, ICartService cartService)
        {
            _userManager = userManager;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var couponCode = CouponSessionHelper.Get(HttpContext);
            var cartDto = await _cartService.GetCartAsync(user.Id, couponCode);

            var vm = new CartIndexViewModel
            {
                Items = cartDto.Items.Select(i => new CartItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Quantity = i.Quantity,
                    StockQuantity = i.StockQuantity,
                    MainImageUrl = i.MainImageUrl,
                    PromoBadgeText = i.PromoBadgeText,
                    PromoBadgeColorHex = i.PromoBadgeColorHex,
                    DiscountedUnitPrice = i.DiscountedUnitPrice
                }).ToList(),
                Pricing = cartDto.Pricing,
                CouponCode = cartDto.CouponCode
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public IActionResult ApplyCoupon(string couponCode)
        {
            if (!string.IsNullOrWhiteSpace(couponCode))
                CouponSessionHelper.Set(HttpContext, couponCode.Trim());

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public IActionResult RemoveCoupon()
        {
            CouponSessionHelper.Remove(HttpContext);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _cartService.AddToCartAsync(user?.Id, productId, quantity);

            return Json(new
            {
                success = result.Success,
                requiresLogin = result.RequiresLogin,
                cartCount = result.CartCount,
                quantity = result.Quantity,
                message = result.Message
            });
        }

        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _cartService.DecreaseQuantityAsync(user.Id, productId);

            return Json(new { success = result.Success, cartCount = result.CartCount, quantity = result.Quantity });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateCart(Dictionary<int, int> quantities)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _cartService.UpdateCartAsync(user.Id, quantities);

            TempData["Success"] = AppMessages.QuantityUpdated;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _cartService.RemoveFromCartAsync(user.Id, productId);

            return Json(new { success = result.Success, cartCount = result.CartCount, message = result.Message });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var user = await _userManager.GetUserAsync(User);
            var items = await _cartService.GetCartItemsAsync(user?.Id);
            return Json(items);
        }

        public IActionResult GetMiniCart()
        {
            return ViewComponent("Cart");
        }
    }
}