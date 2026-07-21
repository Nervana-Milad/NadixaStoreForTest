using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Repositories
{
    public class StockNotificationRepository : IStockNotificationRepository
    {
        private readonly NadixaDbContext _context;

        public StockNotificationRepository(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<HashSet<int>> GetPendingProductIdsAsync(string userId)
        {
            var ids = await _context.StockNotificationRequests
                .Where(r => r.UserId == userId && !r.IsNotified)
                .Select(r => r.ProductId)
                .ToListAsync();

            return ids.ToHashSet();
        }

    }
}
