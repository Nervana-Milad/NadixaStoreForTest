using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nadixa.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CartController(NadixaDbContext context, UserManager<AppUser>userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                return View(new List<CartItemViewModel>());
            }

            var cartItemsVm = cart.Items.Select(item => new CartItemViewModel
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Price = item.Product.Price,
                StockQuantity = item.Quantity,
                MainImageUrl = item.Product.MainImageUrlPath
            }).ToList();

            return View(cartItemsVm);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            // هات المنتج كامل
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Product not found"
                });
            }

            // لو المنتج Out Of Stock
            if (product.StockQuantity <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Product is out of stock"
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

            // الكمية المطلوبة النهائية
            int requestedQuantity =
                (existingItem?.Quantity ?? 0) + quantity;

            // تحقق من الـ stock
            if (requestedQuantity > product.StockQuantity)
            {
                return Json(new
                {
                    success = false,
                    message = $"Only {product.StockQuantity} item(s) available in stock"
                });
            }

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    CartId = cart.Id
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            var cartCount = cart.Items.Sum(i => i.Quantity);

            return Json(new
            {
                success = true,
                cartCount
            });
        }

        //[HttpPost]
        //[Authorize]
        //public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    // ✔️ تحقق من وجود المنتج
        //    var productExists = await _context.Products.AnyAsync(p => p.Id == productId);

        //    if (!productExists)
        //        return Json(new { success = false, message = "Product not found" });

        //    var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == user.Id);

        //    if(cart == null)
        //    {
        //        cart = new Cart
        //        {
        //            UserId = user.Id,
        //            Items = new List<CartItem>()
        //        };

        //        _context.Carts.Add(cart);
        //        await _context.SaveChangesAsync();
        //    }

        //    var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);

        //    if(existingItem != null)
        //    {
        //        existingItem.Quantity += quantity;
        //    }
        //    else
        //    {
        //        var cartItem = new CartItem
        //        {
        //            ProductId = productId,
        //            Quantity = quantity,
        //            CartId = cart.Id
        //        };
        //        _context.CartItems.Add(cartItem);
        //    }

        //    await _context.SaveChangesAsync();
        //    //return RedirectToAction("Index");

        //    var cartCount = cart.Items.Sum(i => i.Quantity);

        //    return Json(new
        //    {
        //        success = true,
        //        cartCount
        //    });

        //}

        [HttpPost]
        public async Task<IActionResult> UpdateCart(Dictionary<int, int> quantities)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == user.Id);

            if(cart == null)
            {
                return RedirectToAction("Index");
            }

            foreach(var item in cart.Items)
            {
                if (quantities.ContainsKey(item.ProductId))
                {
                    var requestedQuantity = quantities[item.ProductId];

                    // منع القيم الأقل من 1
                    if (requestedQuantity < 1)
                    {
                        requestedQuantity = 1;
                    }

                    // منع تجاوز المخزون
                    if (requestedQuantity > item.Product.StockQuantity)
                    {
                        requestedQuantity = item.Product.StockQuantity;
                    }

                    item.Quantity = requestedQuantity;
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            var item = cart.Items
                .FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            var cartCount = cart.Items.Sum(i => i.Quantity);

            return Json(new
            {
                success = true,
                cartCount
            });
        }


        public IActionResult GetMiniCart()
        {
            return ViewComponent("Cart");
        }
    }
}
