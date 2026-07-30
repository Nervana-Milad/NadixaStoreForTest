using Microsoft.EntityFrameworkCore;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services

{
    public class CouponService : ICouponService
    {
        private readonly NadixaDbContext _context;

        public CouponService(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<(bool isValid, decimal discountAmount, string? error, Coupon? coupon)>
            ValidateAndCalculateAsync(string code, string userId, decimal orderSubtotal, bool isUserFirstOrder)
        {
            var coupon = await _context.Coupons
                .Include(c => c.Usages)
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.ToUpper());

            if (coupon == null)
                return (false, 0, "الكود غير موجود", null);

            if (!coupon.IsCurrentlyValid)
                return (false, 0, "الكود منتهي أو غير مفعّل", null);

            if (coupon.FirstOrderOnly && !isUserFirstOrder)
                return (false, 0, "الكود ده لأول عملية شراء بس", null);

            if (coupon.MinOrderAmount.HasValue && orderSubtotal < coupon.MinOrderAmount.Value)
                return (false, 0, $"الحد الأدنى لاستخدام الكود {coupon.MinOrderAmount.Value} جنيه", null);

            if (coupon.MaxTotalUsage.HasValue && coupon.Usages.Count >= coupon.MaxTotalUsage.Value)
                return (false, 0, "الكود وصل للحد الأقصى من الاستخدام", null);

            var userUsageCount = coupon.Usages.Count(u => u.UserId == userId);
            if (coupon.MaxUsagePerUser.HasValue && userUsageCount >= coupon.MaxUsagePerUser.Value)
                return (false, 0, "لقد استخدمت هذا الكود من قبل", null);

            decimal discount = coupon.DiscountType == CouponDiscountType.Percentage
                ? orderSubtotal * (coupon.Value / 100)
                : coupon.Value;

            if (coupon.MaxDiscountAmount.HasValue)
                discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);

            discount = Math.Min(discount, orderSubtotal);

            return (true, Math.Round(discount, 2), null, coupon);
        }

        public async Task RegisterUsageAsync(int couponId, string userId, int orderId, decimal discountApplied)
        {
            _context.CouponUsages.Add(new CouponUsage
            {
                CouponId = couponId,
                UserId = userId,
                OrderId = orderId,
                DiscountApplied = discountApplied,
                UsedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }

        public async Task<List<Coupon>> GetAllAsync()
        {
            return await _context.Coupons.OrderByDescending(c => c.Id).ToListAsync();
        }

        public async Task<Coupon> CreateAsync(Coupon coupon)
        {
            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();
            return coupon;
        }

        public async Task<bool> UpdateAsync(Coupon coupon)
        {
            _context.Coupons.Update(coupon);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return false;

            _context.Coupons.Remove(coupon);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return false;

            coupon.IsActive = !coupon.IsActive;
            return await _context.SaveChangesAsync() > 0;
        }


  

        public async Task<Coupon?> GetByIdAsync(int id)      // ⬅️ الميثود الجديدة
        {
            return await _context.Coupons.FindAsync(id);
        }
    }
}
