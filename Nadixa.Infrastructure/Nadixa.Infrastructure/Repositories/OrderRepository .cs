using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;

namespace Nadixa.Infrastructure.Repositories
{ 
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly NadixaDbContext _context;

        public OrderRepository(NadixaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Order?> GetOrderWithItemsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // ملحوظة: نفس النتيجة ممكن تتحقق برضو بـ:
        // await GetByIdAsync(id, o => o.OrderItems);
        // لكن مش هيعمل ThenInclude للـ Product جوه الـ OrderItems،
        // فسبنا الميثود المخصصة دي عشان الـ nested include
    }
}
