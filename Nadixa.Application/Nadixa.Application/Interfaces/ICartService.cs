using Nadixa.Application.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartViewDto> GetCartAsync(string userId, string? couponCode);
        Task<CartActionResult> AddToCartAsync(string? userId, int productId, int quantity);
        Task<CartActionResult> DecreaseQuantityAsync(string userId, int productId);
        Task UpdateCartAsync(string userId, Dictionary<int, int> quantities);
        Task<CartActionResult> RemoveFromCartAsync(string userId, int productId);
        Task<Dictionary<int, int>> GetCartItemsAsync(string? userId);

    }
}
