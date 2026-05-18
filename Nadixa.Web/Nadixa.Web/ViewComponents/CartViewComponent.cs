using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public CartViewComponent(NadixaDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return View(new CartMiniViewModel());
            }

            var cart = await _context.Carts.Include(c => c.Items).ThenInclude(ci => ci.Product).FirstOrDefaultAsync(c => c.UserId == user.Id);

            var vm = new CartMiniViewModel();

            if(cart != null)
            {
                vm.Items = cart.Items.Select(i => new CartItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Price = i.Product.Price,

                    // دي كمية المستخدم في الكارت
                    Quantity = i.Quantity,

                    // دي كمية المخزون
                    StockQuantity = i.Product.StockQuantity,

                    MainImageUrl = i.Product.MainImageUrlPath,
                }).ToList();
            }
            return View(vm);
        }
    }
}
