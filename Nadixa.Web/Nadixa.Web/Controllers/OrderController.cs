using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
using Nadixa.Infrastructure.Services;
using Nadixa.Web.Helpers;
using Nadixa.Web.Models.ViewModels;
using Nadixa.Web.Services;
using Nadixa.Core.Common;

namespace Nadixa.Web.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailSender _emailSender;
        private readonly IRazorViewRenderer _viewRenderer;
        private readonly IPricingEngine _pricingEngine;
        private readonly ICouponService _couponService;
        private readonly ILoyaltyService _loyaltyService;

        // الأساس بس - بعد كده الـ PricingEngine بيقرر لو هيتخصم أو يتلغى تمامًا
        private const decimal BASE_SHIPPING_FEE = 50m;

        public OrderController(
            NadixaDbContext context,
            UserManager<AppUser> userManager,
            IUnitOfWork unitOfWork,
            EmailSender emailSender,
            IRazorViewRenderer viewRenderer,
            IPricingEngine pricingEngine,
            ICouponService couponService,
            ILoyaltyService loyaltyService)
        {
            _context = context;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _viewRenderer = viewRenderer;
            _pricingEngine = pricingEngine;
            _couponService = couponService;
            _loyaltyService = loyaltyService;
        }

        // ==============================================
        // 1. GET: Checkout / Payment Summary Page
        // ==============================================
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var couponCode = CouponSessionHelper.Get(HttpContext);
            var pricing = await CalculatePricingAsync(user.Id, cart, couponCode);

            var model = new CheckoutVM
            {
                FullName = $"{user.FirstName} {user.LastName}",
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,

                CouponCode = couponCode
            };

            ApplyPricingToVm(model, pricing);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout(CheckoutVM model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // الكود بيتاخد من الـ Session (اللي طُبق في صفحة السلة) مش من الفورم،
            // عشان مايتغيرش من غير ما الـ PricingEngine يتحقق منه
            var couponCode = CouponSessionHelper.Get(HttpContext);
            var pricing = await CalculatePricingAsync(user.Id, cart, couponCode);

            if (!ModelState.IsValid)
            {
                ApplyPricingToVm(model, pricing);
                return View(model);
            }

            // Check Stock Availability
            foreach (var item in cart.Items)
            {
                if (item.Quantity > item.Product.StockQuantity)
                {
                    ModelState.AddModelError("",
                        $"Only {item.Product.StockQuantity} item(s) available for {item.Product.Name}");
                }
            }

            if (!ModelState.IsValid)
            {
                ApplyPricingToVm(model, pricing);
                return View(model);
            }

            // Save address to user profile
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.City = model.City;

            await _userManager.UpdateAsync(user);

            // Create Order (مع حفظ تفاصيل الخصم كـ Snapshot - راجعي ORDER_ENTITY_CHANGES.txt)
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

                SubTotal = pricing.SubTotal,
                DiscountAmount = pricing.ProductsDiscountTotal + pricing.BundleDiscountTotal
                                 + pricing.CouponDiscount + pricing.LoyaltyDiscount,
                ShippingFee = pricing.ShippingFee,
                CouponCode = pricing.CouponDiscount > 0 ? couponCode : null,

                TotalPrice = pricing.GrandTotal
            };

            await _unitOfWork.Repository<Order>().AddAsync(order);
            await _unitOfWork.CompleteAsync();

            // ── Send order confirmation email ─────────────────────────────
            try
            {
                var emailModel = new OrderConfirmationViewModel
                {
                    OrderId = order.Id,
                    CustomerName = model.FullName,
                    Address = model.Address,
                    City = model.City,
                    PhoneNumber = model.PhoneNumber,
                    Notes = model.Notes,
                    OrderDate = DateTime.Now,
                    SubTotal = pricing.SubTotal,
                    ShippingFee = pricing.ShippingFee,
                    GrandTotal = order.TotalPrice,
                    TotalDiscount = pricing.ProductsDiscountTotal + pricing.BundleDiscountTotal + pricing.CouponDiscount,
                    CouponCode = order.CouponCode,
                    Items = cart.Items.Select(i => new OrderConfirmationItem
                    {
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        Price = i.Product.Price
                    }).ToList()
                };

                var emailBody = await _viewRenderer.RenderAsync(
                    "Emails/OrderConfirmation", emailModel);

                _emailSender.SendEmail(
                    senderName: "Nadixa Store",
                    senderEmail: "your-email@gmail.com",
                    toName: model.FullName,
                    toEmail: user.Email!,
                    subject: $"Order Confirmation #{order.Id}",
                    textContent: emailBody
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email failed: {ex.Message}");
            }

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

                item.Product.StockQuantity -= item.Quantity;
            }

            await _unitOfWork.CompleteAsync();

            // تسجيل استخدام الكوبون فعليًا (بعد ما الأوردر اتأكد)
            if (pricing.CouponDiscount > 0 && !string.IsNullOrWhiteSpace(couponCode))
            {
                var validationResult = await _couponService.ValidateAndCalculateAsync(
       couponCode, user.Id, pricing.SubTotal, false);

                var isValid = validationResult.IsValid;
                var coupon = validationResult.Coupon;
                var discountAmount = validationResult.DiscountAmount;
                var error = validationResult.Error;

                if (isValid && coupon != null)
                {
                    await _couponService.RegisterUsageAsync(coupon.Id, user.Id, order.Id, pricing.CouponDiscount);
                }
            }

            // خصم نقاط الولاء المستبدلة (لو العميل استبدل نقاط)
            if (pricing.LoyaltyDiscount > 0)
            {
                var pointsUsed = (int)Math.Round(pricing.LoyaltyDiscount / 0.10m); // لازم تتطابق مع EgpValuePerPointRedeemed
                await _loyaltyService.RedeemPointsAsync(user.Id, pointsUsed, order.Id);
            }

            // إضافة نقاط ولاء جديدة عن الأوردر ده
            await _loyaltyService.AddPointsForOrderAsync(user.Id, order.Id, order.TotalPrice);

            // Clear Cart + الكوبون من الـ Session
            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();
            CouponSessionHelper.Remove(HttpContext);

            TempData["Success"] = AppMessages.OrderPlacedSuccessfully;
            return RedirectToAction("Success", new { id = order.Id });
        }

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

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return RedirectToAction("Login", "Auth");

            var order = await _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null) return NotFound();

            var orderDetailsViewModel = new OrderDetailsViewModel
            {
                OrderId = order.Id,
                FullName = order.FullName,
                Address = order.Address,
                Phone = order.PhoneNumber,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                ShippingFee = order.ShippingFee,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                GrandTotal = order.TotalPrice,
                Items = order.OrderItems.Select(item => new OrderItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ImageUrl = item.Product.MainImageUrlPath,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList()
            };

            return View(orderDetailsViewModel);
        }

        [Authorize]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders.Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == OrderStatus.Shipped ||
                order.Status == OrderStatus.Delivered)
            {
                TempData["Error"] = "This order cannot be cancelled";
                return RedirectToAction("Details", new { id });
            }

            foreach (var item in order.OrderItems)
            {
                item.Product.StockQuantity += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled;

            await _context.SaveChangesAsync();

            TempData["Success"] = AppMessages.CancelOrder;
            return RedirectToAction("Details", new { id });
        }

        // ==============================================
        // Helpers
        // ==============================================
        private async Task<CartPricingResult> CalculatePricingAsync(string userId, Cart cart, string? couponCode)
        {
            var request = new CartPricingRequest
            {
                UserId = userId,
                CouponCode = couponCode,
                BaseShippingFee = BASE_SHIPPING_FEE,
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

        private static void ApplyPricingToVm(CheckoutVM model, CartPricingResult pricing)
        {
            model.SubTotal = pricing.SubTotal;
            model.ShippingFee = pricing.ShippingFee;
            model.GrandTotal = pricing.GrandTotal;

            model.ProductsDiscount = pricing.ProductsDiscountTotal;
            model.BundleDiscount = pricing.BundleDiscountTotal;
            model.CouponDiscount = pricing.CouponDiscount;
            model.ShippingDiscount = pricing.ShippingDiscount;
            model.CouponError = pricing.CouponError;
            model.LoyaltyPointsToEarn = pricing.LoyaltyPointsToEarn;
            model.AppliedPromotions = pricing.AppliedPromotions;
        }
    }
}
