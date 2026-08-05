using Nadixa.Application.DTOS.Wishlist;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<List<WishlistItemDto>> GetWishlistAsync(string userId);
        Task<WishlistToggleResult> ToggleAsync(string? userId, int productId);
    }
}
