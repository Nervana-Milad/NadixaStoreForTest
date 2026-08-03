using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;

namespace Nadixa.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly NadixaDbContext _context;

        public CartRepository(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartWithItemsAndProductsAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>()
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<int> GetCartCountAsync(int cartId)
        {
            return await _context.CartItems
                .Where(i => i.CartId == cartId)
                .SumAsync(i => i.Quantity);
        }

        public void AddItem(CartItem item)
        {
            _context.CartItems.Add(item);
        }

        public void RemoveItem(CartItem item)
        {
            _context.CartItems.Remove(item);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}