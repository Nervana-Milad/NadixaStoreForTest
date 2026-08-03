using AutoMapper;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPricingEngine _pricingEngine;
        private readonly IPromotionService _promotionService;
        private readonly IMapper _mapper;

        public CartService(
            IUnitOfWork unitOfWork,
            IPricingEngine pricingEngine,
            IPromotionService promotionService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _pricingEngine = pricingEngine;
            _promotionService = promotionService;
            _mapper = mapper;
        }

        public async Task<CartViewDto> GetCartAsync(string userId, string? couponCode)
        {
            var vm = new CartViewDto { CouponCode = couponCode };

            var cart = await _unitOfWork.Carts.GetCartWithItemsAndProductsAsync(userId);

            if (cart == null || !cart.Items.Any(i => i.Product != null))
                return vm;

            var activePromotions = await _promotionService.GetActivePromotionsAsync();

            vm.Items = cart.Items
                .Where(item => item.Product != null)
                .Select(item => MapCartItemWithPromo(item, activePromotions))
                .ToList();

            vm.Pricing = await CalculatePricingAsync(userId, cart, couponCode);

            return vm;
        }

        public async Task<CartActionResult> AddToCartAsync(string? userId, int productId, int quantity)
        {
            if (string.IsNullOrEmpty(userId))
                return new CartActionResult { Success = false, RequiresLogin = true, Message = "You must be logged in." };

            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
            if (product == null)
                return new CartActionResult { Success = false, Message = "The requested product was not found." };

            if (product.StockQuantity <= 0)
                return new CartActionResult { Success = false, Message = "Product is out of stock." };

            var cart = await _unitOfWork.Carts.GetOrCreateCartAsync(userId);

            var existingItem = cart.Items.FirstOrDefault(ci => ci.ProductId == productId);
            int requestedQuantity = (existingItem?.Quantity ?? 0) + quantity;

            if (requestedQuantity > product.StockQuantity)
                return new CartActionResult { Success = false, Message = $"Only {product.StockQuantity} item(s) available in stock" };

            CartItem item;
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                item = existingItem;
            }
            else
            {
                item = new CartItem { ProductId = productId, Quantity = quantity, CartId = cart.Id };
                _unitOfWork.Carts.AddItem(item);
                cart.Items.Add(item);
            }

            await _unitOfWork.Carts.SaveChangesAsync();

            return new CartActionResult
            {
                Success = true,
                CartCount = cart.Items.Sum(i => i.Quantity),
                Quantity = item.Quantity,
                Message = "Added to cart."
            };
        }

        public async Task<CartActionResult> DecreaseQuantityAsync(string userId, int productId)
        {
            var cart = await _unitOfWork.Carts.GetCartWithItemsAndProductsAsync(userId);
            if (cart == null)
                return new CartActionResult { Success = false };

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
                return new CartActionResult { Success = false };

            if (item.Quantity > 1)
                item.Quantity--;
            else
                _unitOfWork.Carts.RemoveItem(item);

            await _unitOfWork.Carts.SaveChangesAsync();

            var cartCount = await _unitOfWork.Carts.GetCartCountAsync(cart.Id);
            var currentQuantity = cart.Items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0;

            return new CartActionResult { Success = true, CartCount = cartCount, Quantity = currentQuantity };
        }

        public async Task UpdateCartAsync(string userId, Dictionary<int, int> quantities)
        {
            var cart = await _unitOfWork.Carts.GetCartWithItemsAndProductsAsync(userId);
            if (cart == null) return;

            foreach (var item in cart.Items)
            {
                if (quantities.TryGetValue(item.ProductId, out var requestedQuantity))
                {
                    if (requestedQuantity < 1)
                        requestedQuantity = 1;

                    if (requestedQuantity > item.Product.StockQuantity)
                        requestedQuantity = item.Product.StockQuantity;

                    item.Quantity = requestedQuantity;
                }
            }

            await _unitOfWork.Carts.SaveChangesAsync();
        }

        public async Task<CartActionResult> RemoveFromCartAsync(string userId, int productId)
        {
            var cart = await _unitOfWork.Carts.GetCartWithItemsAndProductsAsync(userId);
            if (cart == null)
                return new CartActionResult { Success = false };

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _unitOfWork.Carts.RemoveItem(item);
                await _unitOfWork.Carts.SaveChangesAsync();
            }

            var cartCount = await _unitOfWork.Carts.GetCartCountAsync(cart.Id);

            return new CartActionResult { Success = true, CartCount = cartCount, Message = "Cart Item Deleted successfully." };
        }


        public async Task<Dictionary<int, int>> GetCartItemsAsync(string? userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new Dictionary<int, int>();

            var cart = await _unitOfWork.Carts.GetOrCreateCartAsync(userId);
            return cart.Items.ToDictionary(x => x.ProductId, x => x.Quantity);
        }

        // ===== Helper Methods =====

        // 👇 هنا المكان اللي بيدمج AutoMapper (للحقول البسيطة) + Manual Logic (للـ Promotion)
        private CartItemDto MapCartItemWithPromo(CartItem item, IEnumerable<Promotion> activePromotions)
        {
            var dto = _mapper.Map<CartItemDto>(item);   // 👈 AutoMapper بيملى الحقول البسيطة بس

            var product = item.Product;

            var promo = activePromotions
                .Where(p =>
                    !p.IsFirstPurchaseOnly &&
                    (p.Scope == PromotionScope.AllProducts ||
                     (p.Scope == PromotionScope.Category && p.ProductCategoryId == product.ProductCategoryId) ||
                     (p.Scope == PromotionScope.SubCategory && p.ProductSubCategoryId == product.ProductSubCategoryId) ||
                     (p.Scope == PromotionScope.SpecificProduct && p.ProductId == product.Id)))
                .OrderByDescending(p => p.Priority)
                .FirstOrDefault();

            // 👇 المنطق المعقد (قرار الـ Promotion) بيتحط يدوي بعد الـ AutoMapper
            dto.PromoBadgeText = promo?.BadgeText;
            dto.PromoBadgeColorHex = promo?.BadgeColorHex;
            dto.DiscountedUnitPrice = (promo != null && promo.Type != PromotionType.BuyXGetYFree)
                ? _promotionService.CalculateDiscountedPrice(product.Price, promo)
                : null;
            return dto;
        }
    

    private async Task<CartPricingResult> CalculatePricingAsync(string userId, Cart cart, string? couponCode)
        {
            var request = new CartPricingRequest
            {
                UserId = userId,
                CouponCode = couponCode,
                Items = cart.Items.Where(i => i.Product != null).Select(i => new CartLineItem
                {
                    ProductId = i.ProductId,
                    ProductCategoryId = i.Product.ProductCategoryId,
                    ProductSubCategoryId = i.Product.ProductSubCategoryId,
                    UnitPrice = i.Product.Price,
                    Quantity = i.Quantity
                }).ToList()
            };

            return await _pricingEngine.CalculateAsync(request);
        }
    }
    }