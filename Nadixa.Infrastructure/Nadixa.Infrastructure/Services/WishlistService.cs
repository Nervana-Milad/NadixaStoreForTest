using AutoMapper;
using Nadixa.Application.DTOS.Wishlist;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public WishlistService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<WishlistItemDto>> GetWishlistAsync(string userId)
        {
            var wishlist = await _unitOfWork.Wishlists.GetWishlistWithItemsAndProductsAsync(userId);

            if (wishlist == null)
                return new List<WishlistItemDto>();

            var validItems = wishlist.Items.Where(i => i.Product != null);

            var items = _mapper.Map<List<WishlistItemDto>>(validItems);

            // 👇 جديد: نجيب كميات الكارت الحالية بتاعت نفس اليوزر
            var cart = await _unitOfWork.Carts.GetCartWithItemsAndProductsAsync(userId);
            var cartQuantities = cart?.Items.ToDictionary(i => i.ProductId, i => i.Quantity) ?? new Dictionary<int, int>();

            foreach (var item in items)
            {
                item.CartQuantity = cartQuantities.ContainsKey(item.ProductId) ? cartQuantities[item.ProductId] : 0;
            }

            return items;
        }

        public async Task<WishlistToggleResult> ToggleAsync(string? userId, int productId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new WishlistToggleResult
                {
                    Success = false,
                    RequiresLogin = true,
                    Message = "You must be logged in."
                };
            }

            var wishlist = await _unitOfWork.Wishlists.GetOrCreateWishlistAsync(userId);

            var existingItem = wishlist.Items.FirstOrDefault(i => i.ProductId == productId);
            bool isAdded;
            if (existingItem != null)
            {
                _unitOfWork.Wishlists.RemoveItem(existingItem);
                wishlist.Items.Remove(existingItem);
                isAdded = false;
            }
            else
            {
                var newItem = new WishlistItem { ProductId = productId, WishlistId = wishlist.Id };
                _unitOfWork.Wishlists.AddItem(newItem);
                wishlist.Items.Add(newItem);
                isAdded = true;
            }

            await _unitOfWork.Wishlists.SaveChangesAsync();
            return new WishlistToggleResult
            {
                Success = true,
                IsAdded = isAdded,
                Count = wishlist.Items.Count,
                Message = isAdded ? AppMessages.WishlistAdded : AppMessages.WishlistRemoved
            };
        }
    }
}
