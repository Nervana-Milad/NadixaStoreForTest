using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartWithItemsAndProductsAsync(string userId);
        Task<Cart> GetOrCreateCartAsync(string userId);
        Task<int> GetCartCountAsync(int cartId);
        void AddItem(CartItem item);
        void RemoveItem(CartItem item);
        Task SaveChangesAsync();
    }
}
