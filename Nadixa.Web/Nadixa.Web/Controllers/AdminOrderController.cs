using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Nadixa.Application.DTOS.order;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Core.Entities;
using Nadixa.Web.Filters;
using Nadixa.Web.Models.ViewModels;

namespace Nadixa.Web.Controllers
{
    [RequirePermission("EditOrderStatus")]
    public class AdminOrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<AppUser> _userManager;

        public AdminOrderController(IOrderService orderService, UserManager<AppUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            var orders = await _orderService.GetAllOrdersForAdminAsync();

            var model = orders.Select(o => new AdminOrderViewModel
            {
                Id = o.Id,
                CustomerName = o.CustomerName,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                AvailableStatuses = o.AvailableStatuses,
                GrandTotal = o.GrandTotal
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id);
            if (order == null)
                return NotFound();

            var model = new OrderDetailsViewModel
            {
                OrderId = order.OrderId,
                FullName = order.FullName,
                Address = order.Address,
                Phone = order.Phone,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                SubTotal = order.SubTotal,
                GrandTotal = order.GrandTotal,
                Items = order.Items.Select(i => new OrderItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ImageUrl = i.ImageUrl,
                    Quantity = i.Quantity,
                    Price = i.Price
                }).ToList()
            };

            ViewBag.Statuses = order.AvailableStatuses;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            var admin = await _userManager.GetUserAsync(User);

            var result = await _orderService.UpdateOrderStatusAsync(new UpdateOrderStatusDto
            {
                OrderId = orderId,
                Status = status,
                AdminUserName = admin?.UserName
            });

            if (result.Success)
                TempData["Success"] = AppMessages.StatusUpdatedSuccess;
            else
                TempData["Error"] = result.ErrorMessage;

            return RedirectToAction(nameof(Details), new { id = orderId });
        }

        [HttpPost]
        public async Task<IActionResult> QuickUpdateStatus(int orderId, string status)
        {
            var admin = await _userManager.GetUserAsync(User);

            var result = await _orderService.UpdateOrderStatusAsync(new UpdateOrderStatusDto
            {
                OrderId = orderId,
                Status = status,
                AdminUserName = admin?.UserName
            });

            TempData[result.Success ? "Success" : "Error"] =
                result.Success ? "Order status updated successfully." : result.ErrorMessage;

            return RedirectToAction(nameof(Index));
        }
    }
}
