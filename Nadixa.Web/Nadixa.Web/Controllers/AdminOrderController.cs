using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using Nadixa.Web.Helpers;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminOrderController : Controller
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;


        public AdminOrderController(NadixaDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var orders = await _context.Orders
                            .OrderByDescending(o => o.CreatedAt)
                            .Select(o => new AdminOrderViewModel
                            {
                                Id = o.Id,
                                CustomerName = o.FullName,
                                CreatedAt = o.CreatedAt,
                                Status = o.Status.ToString(),
                                GrandTotal = o.TotalPrice,
                            })
                            .ToListAsync();
            return View(orders);
        }


        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var model = new OrderDetailsViewModel
            {
                OrderId = order.Id,
                FullName = order.FullName,
                Address = $"{order.Address}, {order.City}",
                Phone = order.PhoneNumber,

                CreatedAt = order.CreatedAt,
                Status = order.Status.ToString(),

                SubTotal = order.TotalPrice,
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
            var availableStatuses = new List<string>();

            switch (model.Status)
            {
                case "Pending":
                    availableStatuses.Add("Pending");
                    availableStatuses.Add("Processing");
                    availableStatuses.Add("Cancelled");
                    break;

                case "Processing":
                    availableStatuses.Add("Processing");
                    availableStatuses.Add("Shipped");
                    availableStatuses.Add("Cancelled");
                    break;

                case "Shipped":
                    availableStatuses.Add("Shipped");
                    availableStatuses.Add("Delivered");
                    break;

                case "Delivered":
                    availableStatuses.Add("Delivered");
                    break;

                case "Cancelled":
                    availableStatuses.Add("Cancelled");
                    break;
            }

            ViewBag.Statuses = availableStatuses;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Delivered و Cancelled حالات نهائية
            if (order.Status == OrderStatus.Cancelled ||
                order.Status == OrderStatus.Delivered)
            {
                return RedirectToAction(nameof(Details),
                    new { id = order.Id });
            }

            if (Enum.TryParse<OrderStatus>(status, out var newStatus))
            {
                bool isValidTransition = order.Status switch
                {
                    OrderStatus.Pending =>
                        newStatus == OrderStatus.Pending ||
                        newStatus == OrderStatus.Processing ||
                        newStatus == OrderStatus.Cancelled,

                    OrderStatus.Processing =>
                        newStatus == OrderStatus.Processing ||
                        newStatus == OrderStatus.Shipped ||
                        newStatus == OrderStatus.Cancelled,

                    OrderStatus.Shipped =>
                        newStatus == OrderStatus.Shipped ||
                        newStatus == OrderStatus.Delivered,

                    _ => false
                };

                if (!isValidTransition)
                {
                    return RedirectToAction(nameof(Details),
                        new { id = order.Id });
                }

                order.Status = newStatus;

                await _context.SaveChangesAsync();
                TempData["Success"] = AppMessages.StatusUpdatedSuccess;

            }

            return RedirectToAction(nameof(Details),
                new { id = orderId });
        }
    }
}
