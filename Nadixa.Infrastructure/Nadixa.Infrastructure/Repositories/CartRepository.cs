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
    public class CartRepository : ICartRepository
    {
        private readonly NadixaDbContext _context;
        public CartRepository(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<int, int>> GetCartItemsAsync(string userId)
        {
            return await _context.Carts
                .Where(c => c.UserId == userId)
                .SelectMany(c => c.Items)
                .ToDictionaryAsync(i => i.ProductId, i => i.Quantity);
        }

    }
}
