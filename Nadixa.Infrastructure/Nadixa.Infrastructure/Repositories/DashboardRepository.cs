using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Core.Models.Dashboard;
using Nadixa.Infrastructure.Data;

namespace Nadixa.Infrastructure.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly NadixaDbContext _context;

    public DashboardRepository(NadixaDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetRevenueAsync(
        DateTime startDate,
        DateTime? endDate = null)
    {
        var query = _context.Orders
            .Where(o =>
                o.CreatedAt >= startDate &&
                o.Status != OrderStatus.Cancelled);

        if (endDate.HasValue)
        {
            query = query.Where(o =>
                o.CreatedAt < endDate.Value);
        }

        return await query
            .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;
    }

    public async Task<int> GetOrderCountAsync(
        DateTime startDate,
        DateTime? endDate = null)
    {
        var query = _context.Orders
            .Where(o => o.CreatedAt >= startDate);

        if (endDate.HasValue)
        {
            query = query.Where(o =>
                o.CreatedAt < endDate.Value);
        }

        return await query.CountAsync();
    }

    public async Task<Dictionary<OrderStatus, int>>
        GetOrdersCountByStatusAsync()
    {
        return await _context.Orders
            .GroupBy(o => o.Status)
            .Select(g => new
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(
                x => x.Status,
                x => x.Count);
    }

    public async Task<int> GetTotalProductsAsync()
    {
        return await _context.Products.CountAsync();
    }

    public async Task<int> GetLowStockProductsCountAsync(
        int threshold)
    {
        return await _context.Products
            .CountAsync(p =>
                p.StockQuantity <= threshold &&
                p.StockQuantity > 0);
    }

    public async Task<int> GetOutOfStockProductsCountAsync()
    {
        return await _context.Products
            .CountAsync(p => p.StockQuantity == 0);
    }

    public async Task<List<TopProductModel>>
        GetTopSellingProductsAsync(int count)
    {
        return await _context.OrderItems
            .Where(oi =>
                oi.Order.Status != OrderStatus.Cancelled)
            .GroupBy(oi => new
            {
                oi.ProductId,
                oi.Product.Name,
                oi.Product.MainImageUrlPath
            })
            .Select(g => new TopProductModel
            {
                Id = g.Key.ProductId,
                Name = g.Key.Name,
                ImageUrl = g.Key.MainImageUrlPath,
                TotalSold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x =>
                    x.Quantity * x.Price)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<RecentOrderModel>>
        GetRecentOrdersAsync(int count)
    {
        return await _context.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .Select(o => new RecentOrderModel
            {
                Id = o.Id,
                CustomerName = o.FullName,
                Total = o.TotalPrice,
                Status = o.Status.ToString(),
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<LowStockProduct>>
        GetLowStockProductsAsync(
            int threshold,
            int count)
    {
        return await _context.Products
            .Where(p =>
                p.StockQuantity <= threshold &&
                p.StockQuantity > 0)
            .OrderBy(p => p.StockQuantity)
            .Take(count)
            .Select(p => new LowStockProduct
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.MainImageUrlPath,
                StockQuantity = p.StockQuantity
            })
            .ToListAsync();
    }

    public async Task<List<DailyRevenueModel>>
        GetDailyRevenueAsync(DateTime startDate)
    {
        return await _context.Orders
            .Where(o =>
                o.CreatedAt >= startDate &&
                o.Status != OrderStatus.Cancelled)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenueModel
            {
                Date = g.Key,
                Revenue = g.Sum(x => x.TotalPrice)
            })
            .OrderBy(x => x.Date)
            .ToListAsync();
    }
}