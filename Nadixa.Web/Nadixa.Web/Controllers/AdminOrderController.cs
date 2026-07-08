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
                .ToListAsync();

            var model = orders.Select(o => new AdminOrderViewModel
            {
                Id = o.Id,
                CustomerName = o.FullName,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                AvailableStatuses = GetAvailableStatuses(o.Status),
                GrandTotal = o.TotalPrice
            }).ToList();

            return View(model);
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
                Status = order.Status,
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

            ViewBag.Statuses = GetAvailableStatuses(order.Status).ToList();

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

            // الحالات النهائية لا يمكن تعديلها
            if (order.Status == OrderStatus.Delivered ||
                order.Status == OrderStatus.Cancelled)
            {
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }

            if (!Enum.TryParse<OrderStatus>(status, out var newStatus))
            {
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }

            var allowedStatuses = GetAvailableStatuses(order.Status);

            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Invalid status transition.";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }
            var admin = await _userManager.GetUserAsync(User);
            order.Status = newStatus;
            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = admin?.UserName ?? "System"
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = AppMessages.StatusUpdatedSuccess;

            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
    private List<OrderStatus> GetAvailableStatuses(OrderStatus currentStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending => new()
        {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Cancelled
        },

                OrderStatus.Confirmed => new()
        {
            OrderStatus.Confirmed,
            OrderStatus.Processing,
            OrderStatus.Cancelled
        },

                OrderStatus.Processing => new()
        {
            OrderStatus.Processing,
            OrderStatus.Shipped
        },

                OrderStatus.Shipped => new()
        {
            OrderStatus.Shipped,
            OrderStatus.OutForDelivery
        },

                OrderStatus.OutForDelivery => new()
        {
            OrderStatus.OutForDelivery,
            OrderStatus.Delivered
        },

                _ => new()
        {
            currentStatus
        }
            };
        }

        [HttpPost]
        public async Task<IActionResult> QuickUpdateStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return NotFound();

            if (!Enum.TryParse<OrderStatus>(status, out var newStatus))
                return RedirectToAction(nameof(Index));

            var allowedStatuses = GetAvailableStatuses(order.Status);

            if (!allowedStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Invalid status transition.";
                return RedirectToAction(nameof(Index));
            }

            var admin = await _userManager.GetUserAsync(User);

            order.Status = newStatus;

            _context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = admin?.UserName ?? "System"
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Order status updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
