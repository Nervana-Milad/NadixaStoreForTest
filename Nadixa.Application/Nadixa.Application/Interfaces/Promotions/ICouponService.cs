using Nadixa.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface ICouponService
    {
        // يتحقق من صلاحية الكود لليوزر ده وعلى إجمالي الأوردر ده، ويرجع الخصم لو صالح
        Task<(bool isValid, decimal discountAmount, string? error, Coupon? coupon)>
            ValidateAndCalculateAsync(string code, string userId, decimal orderSubtotal, bool isUserFirstOrder);

        // يسجل استخدام الكوبون بعد ما الأوردر يتأكد فعليًا (يُستدعى وقت الـ Checkout النهائي)
        Task RegisterUsageAsync(int couponId, string userId, int orderId, decimal discountApplied);

        // لوحة تحكم الأدمن
        Task<List<Coupon>> GetAllAsync();
        Task<Coupon> CreateAsync(Coupon coupon);
        Task<bool> UpdateAsync(Coupon coupon);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
        Task<Coupon?> GetByIdAsync(int id);
    }
}
