using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.DTOs;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Helpers;
using Nadixa.Web.Models.ViewModels;
using System.Threading.Tasks;

namespace Nadixa.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IPricingEngine _pricingEngine;
        private readonly IPromotionService _promotionService;

        public CartController(
            NadixaDbContext context,
            UserManager<AppUser> userManager,
            IPricingEngine pricingEngine,
            IPromotionService promotionService)
        {
            _context = context;
            _userManager = userManager;
            _pricingEngine = pricingEngine;
            _promotionService = promotionService;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            var vm = new CartIndexViewModel();

            if (cart == null || !cart.Items.Any(i => i.Product != null))
            {
                return View(vm);
            }

            // العروض النشطة كلها - هنستخدمها عشان نحدد بادچ كل منتج في السلة
            var activePromotions = await _promotionService.GetActivePromotionsAsync();

            vm.Items = cart.Items.Where(item => item.Product != null)
                .Select(item =>
                {
                    var product = item.Product;

                    var promo = activePromotions
                        .Where(p =>
                            !p.IsFirstPurchaseOnly &&
                            (p.Scope == PromotionScope.AllProducts ||
                             (p.Scope == PromotionScope.Category && p.ProductCategoryId == product.ProductCategoryId) ||
                             (p.Scope == PromotionScope.SubCategory && p.ProductSubCategoryId == product.ProductSubCategoryId) ||
                             (p.Scope == PromotionScope.SpecificProduct && p.ProductId == product.Id)))
                        .OrderByDescending(p => p.Priority)
                        .FirstOrDefault();

                    return new CartItemViewModel
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = item.Quantity,
                        StockQuantity = product.StockQuantity,
                        MainImageUrl = product.MainImageUrlPath,

                        PromoBadgeText = promo?.BadgeText,
                        PromoBadgeColorHex = promo?.BadgeColorHex,
                        DiscountedUnitPrice = (promo != null && promo.Type != PromotionType.BuyXGetYFree)
                            ? _promotionService.CalculateDiscountedPrice(product.Price, promo)
                            : null
                    };
                }).ToList();

            // كود الكوبون المحفوظ في الـ Session (لو العميل طبق واحد قبل كده)
            var couponCode = CouponSessionHelper.Get(HttpContext);

            vm.Pricing = await CalculatePricingAsync(user.Id, cart, couponCode);
            vm.CouponCode = couponCode;

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            if (!string.IsNullOrWhiteSpace(couponCode))
            {
                CouponSessionHelper.Set(HttpContext, couponCode.Trim());
            }

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
            Console.WriteLine("AddToCart Hit");
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

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = AppMessages.ProductNotFound
                });
            }

            if (product.StockQuantity <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = AppMessages.OutOfStock
                });
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    Items = new List<CartItem>()
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.Items
                .FirstOrDefault(ci => ci.ProductId == productId);

            int requestedQuantity =
                (existingItem?.Quantity ?? 0) + quantity;

            if (requestedQuantity > product.StockQuantity)
            {
                return Json(new
                {
                    success = false,
                    message = $"Only {product.StockQuantity} item(s) available in stock"
                });
            }

            CartItem item;

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                item = existingItem;
            }
            else
            {
                item = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    CartId = cart.Id
                };

                _context.CartItems.Add(item);
                cart.Items.Add(item);
            }

            await _context.SaveChangesAsync();

            var cartCount = cart.Items.Sum(i => i.Quantity);

            return Json(new
            {
                success = true,
                cartCount,
                quantity = item.Quantity,
                message = AppMessages.CartAdded
            });
        }

        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Unauthorized();
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return Json(new { success = false });
            }

            var item = cart.Items
                .FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                return Json(new { success = false });
            }

            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                _context.CartItems.Remove(item);
            }

            await _context.SaveChangesAsync();

            var cartCount = await _context.CartItems
                .Where(i => i.CartId == cart.Id)
                .SumAsync(i => i.Quantity);

            var currentItem = await _context.CartItems
                .FirstOrDefaultAsync(i =>
                    i.CartId == cart.Id &&
                    i.ProductId == productId);

            return Json(new
            {
                success = true,
                cartCount,
                quantity = currentItem?.Quantity ?? 0
            });
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateCart(Dictionary<int, int> quantities)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var cart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return RedirectToAction("Index");
            }

            foreach (var item in cart.Items)
            {
                if (quantities.ContainsKey(item.ProductId))
                {
                    var requestedQuantity = quantities[item.ProductId];

                    if (requestedQuantity < 1)
                    {
                        requestedQuantity = 1;
                    }

                    if (requestedQuantity > item.Product.StockQuantity)
                    {
                        requestedQuantity = item.Product.StockQuantity;
                    }

                    item.Quantity = requestedQuantity;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = AppMessages.QuantityUpdated;
            return RedirectToAction("Index");
        }
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (cart == null)
            {
                return Json(new { success = false });
            }
            var item = cart.Items
                .FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            var cartCount = await _context.CartItems
    .Where(i => i.CartId == cart.Id)
    .SumAsync(i => i.Quantity);


            return Json(new
            {
                success = true,
                cartCount,
                message = AppMessages.CartItemDeleted
            });

        }

        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Json(new { });
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return Json(new { });
            }

            return Json(
                cart.Items.ToDictionary(
                    x => x.ProductId,
                    x => x.Quantity
                )
            );
        }
        public IActionResult GetMiniCart()
        {
            return ViewComponent("Cart");
        }

        private async Task<CartPricingResult> CalculatePricingAsync(string userId, Cart cart, string? couponCode)
        {
            var request = new CartPricingRequest
            {
                UserId = userId,
                CouponCode = couponCode,
                Items = cart.Items.Where(i => i.Product != null).Select(i => new CartLineItem
                {
                    ProductId = i.ProductId,
                    ProductCategoryId = i.Product.ProductCategoryId,
                    ProductSubCategoryId = i.Product.ProductSubCategoryId,
                    UnitPrice = i.Product.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return await _pricingEngine.CalculateAsync(request);
        }

    }
}
