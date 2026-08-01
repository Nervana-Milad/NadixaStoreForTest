using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.Dashboard;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;

namespace Nadixa.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly UserManager<AppUser> _userManager;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        UserManager<AppUser> userManager)
    {
        _dashboardRepository = dashboardRepository;
        _userManager = userManager;
    }

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var now = DateTime.UtcNow;

        var thisMonth = new DateTime(
            now.Year,
            now.Month,
            1);

        var lastMonth = thisMonth.AddMonths(-1);

        var today = now.Date;

        var last7Days = today.AddDays(-6);

        var revenueThis =
            await _dashboardRepository.GetRevenueAsync(
                thisMonth);

        var revenueLast =
            await _dashboardRepository.GetRevenueAsync(
                lastMonth,
                thisMonth);

        decimal changePercent =
            revenueLast == 0
                ? 100
                : Math.Round(
                    (revenueThis - revenueLast)
                    / revenueLast * 100,
                    1);

        var ordersByStatus =
            await _dashboardRepository
                .GetOrdersCountByStatusAsync();

        int Get(OrderStatus status)
        {
            return ordersByStatus
                .GetValueOrDefault(status);
        }

        var totalOrdersThisMonth =
            await _dashboardRepository
                .GetOrderCountAsync(thisMonth);

        var totalProducts =
            await _dashboardRepository
                .GetTotalProductsAsync();

        var lowStockCount =
            await _dashboardRepository
                .GetLowStockProductsCountAsync(5);

        var outOfStock =
            await _dashboardRepository
                .GetOutOfStockProductsCountAsync();

        var totalUsers =
            await _userManager.Users.CountAsync();

        var newUsersToday =
            await _userManager.Users
                .CountAsync(u =>
                    u.CreatedAt.Date == today);

        var topProducts =
            await _dashboardRepository
                .GetTopSellingProductsAsync(5);

        var recentOrders =
            await _dashboardRepository
                .GetRecentOrdersAsync(8);

        var lowStockProducts =
            await _dashboardRepository
                .GetLowStockProductsAsync(5, 6);

        var revenueData =
            await _dashboardRepository
                .GetDailyRevenueAsync(last7Days);

        var dailyRevenue =
            Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var date =
                        last7Days.AddDays(i);

                    var revenue =
                        revenueData
                            .FirstOrDefault(x =>
                                x.Date == date)
                            ?.Revenue ?? 0;

                    return new DailyRevenueDto
                    {
                        Day = date.ToString("ddd"),
                        Revenue = revenue
                    };
                })
                .ToList();

        return new DashboardStatsDto
        {
            TotalRevenueThisMonth = revenueThis,
            TotalRevenueLastMonth = revenueLast,
            RevenueChangePercent = changePercent,

            TotalOrdersThisMonth =
                totalOrdersThisMonth,

            PendingOrders =
                Get(OrderStatus.Pending),

            ProcessingOrders =
                Get(OrderStatus.Processing),

            ShippedOrders =
                Get(OrderStatus.Shipped),

            DeliveredOrders =
                Get(OrderStatus.Delivered),

            CancelledOrders =
                Get(OrderStatus.Cancelled),

            TotalProducts =
                totalProducts,

            LowStockCount =
                lowStockCount,

            OutOfStockCount =
                outOfStock,

            TotalUsers =
                totalUsers,

            NewUsersToday =
                newUsersToday,

            TopProducts =
                topProducts.Select(x =>
                    new TopProductDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ImageUrl = x.ImageUrl,
                        TotalSold = x.TotalSold,
                        Revenue = x.Revenue
                    }).ToList(),

            RecentOrders =
                recentOrders.Select(x =>
                    new RecentOrderDto
                    {
                        Id = x.Id,
                        CustomerName = x.CustomerName,
                        Total = x.Total,
                        Status = x.Status,
                        CreatedAt = x.CreatedAt
                    }).ToList(),

            LowStockProducts =
                lowStockProducts.Select(x =>
                    new LowStockProductDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ImageUrl = x.ImageUrl,
                        StockQuantity =
                            x.StockQuantity
                    }).ToList(),

            DailyRevenue =
                dailyRevenue
        };
    }
}