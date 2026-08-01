using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using global::Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nadixa.Application.Interfaces;
using Nadixa.Application.DTOS;
namespace Nadixa.Infrastructure.Services
{

    /// <summary>
    /// نقطة الدخول الوحيدة لأي كونترولر (Cart / Checkout) عشان يحسب السعر النهائي.
    /// بيستدعي كل الخدمات التانية بترتيب معين، ويجمع النتيجة في CartPricingResult واحد.
    /// </summary>
    public class PricingEngine : Application.Interfaces.IPricingEngine
    {
        private readonly IPromotionService _promotionService;
        private readonly IShippingRuleService _shippingRuleService;
        private readonly ICouponService _couponService;
        private readonly ILoyaltyService _loyaltyService;
        private readonly IBundleDealService _bundleDealService;
        private readonly IUserOrderHistoryChecker _orderHistoryChecker;

        public PricingEngine(
            IPromotionService promotionService,
            IShippingRuleService shippingRuleService,
            ICouponService couponService,
            ILoyaltyService loyaltyService,
            IBundleDealService bundleDealService,
            IUserOrderHistoryChecker orderHistoryChecker)
        {
            _promotionService = promotionService;
            _shippingRuleService = shippingRuleService;
            _couponService = couponService;
            _loyaltyService = loyaltyService;
            _bundleDealService = bundleDealService;
            _orderHistoryChecker = orderHistoryChecker;
        }

        public async Task<CartPricingResult> CalculateAsync(CartPricingRequest request)
        {
            var result = new CartPricingResult
            {
                SubTotal = request.Items.Sum(i => i.UnitPrice * i.Quantity)
            };

            var isFirstOrder = await _orderHistoryChecker.IsFirstOrderAsync(request.UserId);
            var activePromotions = await _promotionService.GetActivePromotionsAsync();

            // 1) خصومات المنتجات (نسبة / مبلغ ثابت / Buy X Get Y)
            decimal productsDiscount = 0;

            foreach (var item in request.Items)
            {
                var promo = activePromotions
                    .Where(p =>
                        (!p.IsFirstPurchaseOnly || isFirstOrder) &&
                        (p.Scope == PromotionScope.AllProducts ||
                         (p.Scope == PromotionScope.Category && p.ProductCategoryId == item.ProductCategoryId) ||
                         (p.Scope == PromotionScope.SubCategory && p.ProductSubCategoryId == item.ProductSubCategoryId) ||
                         (p.Scope == PromotionScope.SpecificProduct && p.ProductId == item.ProductId)))
                    .OrderByDescending(p => p.Priority)
                    .FirstOrDefault();

                if (promo == null) continue;

                decimal itemDiscount = 0;

                if (promo.Type == PromotionType.BuyXGetYFree && promo.BuyQuantity > 0 && promo.FreeQuantity > 0)
                {
                    var groupSize = promo.BuyQuantity.Value + promo.FreeQuantity.Value;
                    var freeUnits = (item.Quantity / groupSize) * promo.FreeQuantity.Value;
                    itemDiscount = freeUnits * item.UnitPrice;
                }
                else
                {
                    var discountedUnitPrice = _promotionService.CalculateDiscountedPrice(item.UnitPrice, promo);
                    itemDiscount = (item.UnitPrice - discountedUnitPrice) * item.Quantity;
                }

                if (itemDiscount <= 0) continue;

                productsDiscount += itemDiscount;
                result.AppliedPromotions.Add(new AppliedPromotionInfo
                {
                    Name = promo.Name,
                    BadgeText = promo.BadgeText,
                    DiscountAmount = Math.Round(itemDiscount, 2)
                });
            }

            result.ProductsDiscountTotal = Math.Round(productsDiscount, 2);

            // 2) عروض الباندل (منتجات محددة مع بعض)
            var bundles = await _bundleDealService.GetMatchingBundlesAsync(request.Items);
            result.BundleDiscountTotal = Math.Round(bundles.Sum(b => b.discountAmount), 2);
            foreach (var (bundle, discount) in bundles)
            {
                result.AppliedPromotions.Add(new AppliedPromotionInfo
                {
                    Name = bundle.Name,
                    BadgeText = "عرض باقة",
                    DiscountAmount = Math.Round(discount, 2)
                });
            }

            var netSubtotalAfterProductDiscounts =
                result.SubTotal - result.ProductsDiscountTotal - result.BundleDiscountTotal;

            // 3) الشحن (ببلاش / خصم نسبة حسب إجمالي الأوردر)
            var (finalShipping, shippingDiscount, _) = await _shippingRuleService
                .CalculateShippingAsync(request.BaseShippingFee, netSubtotalAfterProductDiscounts);

            result.ShippingFee = finalShipping;
            result.ShippingDiscount = Math.Round(shippingDiscount, 2);

            // 4) الكوبون (لو العميل دخل كود)
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var couponResult =
                    await _couponService.ValidateAndCalculateAsync(
                        request.CouponCode,
                        request.UserId,
                        netSubtotalAfterProductDiscounts,
                        isFirstOrder);

                var isValid = couponResult.IsValid;
                var discountAmount = couponResult.DiscountAmount;
                var error = couponResult.Error;
                var coupon = couponResult.Coupon;
                if (isValid && coupon != null)
                {
                    result.CouponDiscount = discountAmount;
                    result.AppliedPromotions.Add(new AppliedPromotionInfo
                    {
                        Name = $"كوبون {coupon.Code}",
                        DiscountAmount = discountAmount
                    });
                }
                else
                {
                    result.CouponError = error;
                }
            }

            // 5) نقاط الولاء (لو العميل طلب يستبدل نقاط)
            if (request.RedeemLoyaltyPoints is > 0)
            {
                var balance = await _loyaltyService.GetBalanceAsync(request.UserId);
                var pointsToUse = Math.Min(balance, request.RedeemLoyaltyPoints.Value);
                result.LoyaltyDiscount = _loyaltyService.ConvertPointsToDiscount(pointsToUse);
            }

            // 6) الإجمالي النهائي
            result.GrandTotal = Math.Max(0,
                netSubtotalAfterProductDiscounts
                - result.CouponDiscount
                - result.LoyaltyDiscount
                + result.ShippingFee);

            result.LoyaltyPointsToEarn = _loyaltyService.PreviewPointsToEarn(result.GrandTotal);

            return result;
        }
    }

    /// <summary>
    /// خدمة صغيرة بتجاوب على سؤال واحد: هل ده أول أوردر لليوزر ده؟
    /// (بتحتاج تتنفذ في مشروعك بالرجوع لجدول Orders، فصلتها هنا كـ interface
    /// عشان الـ PricingEngine ميتقفلش على تفاصيل الـ DbContext مباشرة).
    /// </summary>
    public interface IUserOrderHistoryChecker
    {
        Task<bool> IsFirstOrderAsync(string userId);
    }

    public class UserOrderHistoryChecker : IUserOrderHistoryChecker
    {
        private readonly NadixaDbContext _context;

        public UserOrderHistoryChecker(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsFirstOrderAsync(string userId)
        {
            return !await _context.Orders.AnyAsync(o => o.UserId == userId);
        }

    }
}
