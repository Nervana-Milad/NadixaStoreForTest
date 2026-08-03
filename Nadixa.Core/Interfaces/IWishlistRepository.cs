using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IWishlistRepository
    {
        Task<Wishlist?> GetWishlistWithItemsAndProductsAsync(string userId);
        Task<Wishlist> GetOrCreateWishlistAsync(string userId);
        void AddItem(WishlistItem item);
        void RemoveItem(WishlistItem item);
        Task SaveChangesAsync();
    }
}
