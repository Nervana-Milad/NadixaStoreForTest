using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.DTOS
{
    public class DashboardStatsDto
    {
        // Revenue
        public decimal TotalRevenueThisMonth { get; set; }
        public decimal TotalRevenueLastMonth { get; set; }
        public decimal RevenueChangePercent { get; set; }

        // Orders
        public int TotalOrdersThisMonth { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }

        // Products
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }  // StockQuantity <= 5
        public int OutOfStockCount { get; set; }  // StockQuantity == 0

        // Users
        public int TotalUsers { get; set; }
        public int NewUsersToday { get; set; }

        // Top products
        public List<TopProductDto> TopProducts { get; set; } = new();

        // Recent orders
        public List<RecentOrderDto> RecentOrders { get; set; } = new();

        // Low stock alerts
        public List<LowStockProductDto> LowStockProducts { get; set; } = new();

        // Revenue chart (last 7 days)
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
    }
}
