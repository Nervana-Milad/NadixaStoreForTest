using Nadixa.Core.Entities;
using Nadixa.Core.Models.Dashboard;

namespace Nadixa.Core.Interfaces;

public interface IDashboardRepository
{
    Task<decimal> GetRevenueAsync(
        DateTime startDate,
        DateTime? endDate = null);

    Task<int> GetOrderCountAsync(
        DateTime startDate,
        DateTime? endDate = null);

    Task<Dictionary<OrderStatus, int>> GetOrdersCountByStatusAsync();

    Task<int> GetTotalProductsAsync();

    Task<int> GetLowStockProductsCountAsync(int threshold);

    Task<int> GetOutOfStockProductsCountAsync();

    Task<List<TopProductModel>> GetTopSellingProductsAsync(int count);

    Task<List<RecentOrderModel>> GetRecentOrdersAsync(int count);

    Task<List<LowStockProduct>> GetLowStockProductsAsync(
        int threshold,
        int count);

    Task<List<DailyRevenueModel>> GetDailyRevenueAsync(
        DateTime startDate);
}