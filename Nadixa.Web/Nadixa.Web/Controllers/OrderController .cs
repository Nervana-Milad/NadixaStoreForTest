using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        // لنفترض أن مصاريف الشحن ثابتة 50 جنيه
        private const decimal SHIPPING_FEE = 50m;

        public OrderController(
            NadixaDbContext context,
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _context = context;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        // ==============================================
        // 1. GET: Checkout Page (تم التعديل هنا)
        // ==============================================
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);

            // جلب السلة الخاصة بالمستخدم
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // لو السلة فاضية نرجعه لصفحة السلة
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // حساب الأسعار
            decimal subTotal = cart.Items.Sum(x => x.Product.Price * x.Quantity);

            var model = new CheckoutVM
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,

                SubTotal = subTotal,
                ShippingFee = SHIPPING_FEE,
                GrandTotal = subTotal + SHIPPING_FEE
            };

            // نبعت الموديل المحمل بالأسعار للصفحة
            return View(model);
        }

        // ==============================================
        // 2. POST: Checkout (تمت إضافة حماية الأخطاء)
        // ==============================================
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            var user = await _userManager.GetUserAsync(User);

            // جلب السلة
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // [مهم جداً]: لو العميل نسي يكتب حقل في الفورم، لازم نحسب السعر تاني قبل ما نرجعه للصفحة
            if (!ModelState.IsValid)
            {
                model.SubTotal = cart.Items.Sum(x => x.Product.Price * x.Quantity);
                model.ShippingFee = SHIPPING_FEE;
                model.GrandTotal = model.SubTotal + SHIPPING_FEE;
                return View(model);
            }

            decimal subTotal = cart.Items.Sum(x => x.Product.Price * x.Quantity);

            // Save address to user profile
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;

            await _userManager.UpdateAsync(user);


            // Create Order
            var order = new Order
            {
                UserId = user.Id,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City,
                Notes = model.Notes,
                CreatedAt = DateTime.Now,
                Status = OrderStatus.Pending,
                // تم تعديل السعر ليصبح شامل مصاريف الشحن
                TotalPrice = subTotal + SHIPPING_FEE
            };

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // Create OrderItems
            foreach (var item in cart.Items)
            {
                await _unitOfWork.Repository<OrderItem>().AddAsync(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                });
            }

            await _unitOfWork.CompleteAsync();

            // Clear Cart
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            // تمرير الـ id الخاص بالطلب الجديد للـ Success Page
            return RedirectToAction("Success", new { id = order.Id });
        }

        // Success Page
        public async Task<IActionResult> Success(int id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.CustomerName = order.FullName;
            ViewBag.OrderId = order.Id;

            return View();
        }
    }
}