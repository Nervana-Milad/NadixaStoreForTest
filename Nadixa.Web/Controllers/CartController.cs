using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;
using System.Security.Claims;

namespace Nadixa.Web.Controllers
{
    //[Authorize] // Only logged-in users
    public class CartController : Controller
    {
        private readonly NadixaDbContext _context;
        public CartController(NadixaDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefault(c => c.UserId == userId);

            if(cart == null)
            {
                return View(new List<CartItemViewModel>()); // Empty cart
            }

            var model = cart.CartItems.Select(i => new CartItemViewModel
            {
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                MainImageUrl = i.Product.Images.FirstOrDefault()?.ImageUrl ?? "/images/no-image.png",
                Quantity = i.Quantity,
                Price = i.Product.Price
            }).ToList();

            return View(model);
        }

        //Add product to cart               مش شغالة
       [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            var item = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
