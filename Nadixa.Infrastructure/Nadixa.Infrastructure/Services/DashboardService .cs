using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nadixa.Core.DTOS;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
namespace Nadixa.Infrastructure.Services
{
    public class DashboardService : Core.Interfaces.IDashboardService
    {
        private readonly NadixaDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DashboardService(NadixaDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<DashboardStatsDto> GetStatsAsync()
        {
            var now = DateTime.UtcNow;
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);
            var today = now.Date;
            var last7Days = today.AddDays(-6);

            // ── Revenue ───────────────────────────────────────────────
            var revenueThis = await _context.Orders
                .Where(o => o.CreatedAt >= thisMonth
                         && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            var revenueLast = await _context.Orders
                .Where(o => o.CreatedAt >= lastMonth
                         && o.CreatedAt < thisMonth
                         && o.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            decimal changePercent = revenueLast == 0 ? 100 :
                Math.Round((revenueThis - revenueLast) / revenueLast * 100, 1);

            // ── Orders by status ──────────────────────────────────────
            var ordersByStatus = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int Get(OrderStatus s) => ordersByStatus.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

            int totalOrdersThisMonth = await _context.Orders
                .CountAsync(o => o.CreatedAt >= thisMonth);

            // ── Products ──────────────────────────────────────────────
            var totalProducts = await _context.Products.CountAsync();
            var lowStockCount = await _context.Products.CountAsync(p => p.StockQuantity <= 5 && p.StockQuantity > 0);
            var outOfStock = await _context.Products.CountAsync(p => p.StockQuantity == 0);

            // ── Users ─────────────────────────────────────────────────
            var totalUsers = _userManager.Users.Count();
            var newUsersToday = _userManager.Users
                .Count(u => u.CreatedAt.Date == today);

            // ── Top 5 best-selling products ───────────────────────────
            var topProducts = await _context.OrderItems
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.MainImageUrlPath })
                .Select(g => new TopProductDto
                {
                    Id = g.Key.ProductId,
                    Name = g.Key.Name,
                    ImageUrl = g.Key.MainImageUrlPath,
                    TotalSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.Price)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            // ── Recent 8 orders ───────────────────────────────────────
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(8)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    CustomerName = o.FullName,
                    Total = o.TotalPrice,
                    Status = o.Status.ToString(),
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            // ── Low stock alert list ──────────────────────────────────
            var lowStockList = await _context.Products
                .Where(p => p.StockQuantity <= 5)
                .OrderBy(p => p.StockQuantity)
                .Take(6)
                .Select(p => new LowStockProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.MainImageUrlPath,
                    StockQuantity = p.StockQuantity
                })
                .ToListAsync();

            // ── Daily revenue last 7 days ─────────────────────────────
            var last7Revenue = await _context.Orders
                .Where(o => o.CreatedAt >= last7Days
                         && o.Status != OrderStatus.Cancelled)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.TotalPrice) })
                .ToListAsync();

            var dailyRevenue = Enumerable.Range(0, 7).Select(i =>
            {
                var date = last7Days.AddDays(i);
                var rev = last7Revenue.FirstOrDefault(x => x.Date == date)?.Revenue ?? 0;
                return new DailyRevenueDto
                {
                    Day = date.ToString("ddd"),
                    Revenue = rev
                };
            }).ToList();

            return new DashboardStatsDto
            {
                TotalRevenueThisMonth = revenueThis,
                TotalRevenueLastMonth = revenueLast,
                RevenueChangePercent = changePercent,
                TotalOrdersThisMonth = totalOrdersThisMonth,
                PendingOrders = Get(OrderStatus.Pending),
                ProcessingOrders = Get(OrderStatus.Processing),
                ShippedOrders = Get(OrderStatus.Shipped),
                DeliveredOrders = Get(OrderStatus.Delivered),
                CancelledOrders = Get(OrderStatus.Cancelled),
                TotalProducts = totalProducts,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStock,
                TotalUsers = totalUsers,
                NewUsersToday = newUsersToday,
                TopProducts = topProducts,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockList,
                DailyRevenue = dailyRevenue
            };
        }
    }
}
