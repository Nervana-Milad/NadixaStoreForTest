using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly NadixaDbContext _context;

        public WishlistRepository(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<Wishlist?> GetWishlistWithItemsAndProductsAsync(string userId)
        {
            return await _context.Wishlists
                .Include(w => w.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<Wishlist> GetOrCreateWishlistAsync(string userId)
        {
            var wishlist = await _context.Wishlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wishlist == null)
            {
                wishlist = new Wishlist { UserId = userId };
                _context.Wishlists.Add(wishlist);
                await _context.SaveChangesAsync();
            }

            return wishlist;
        }

        public void AddItem(WishlistItem item)
        {
            _context.WishlistItems.Add(item);
        }

        public void RemoveItem(WishlistItem item)
        {
            _context.WishlistItems.Remove(item);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
