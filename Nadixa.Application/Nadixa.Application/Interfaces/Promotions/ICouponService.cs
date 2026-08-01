using Nadixa.Application.DTOS.Coupons;
using Nadixa.Application.DTOS.Coupons;


namespace Nadixa.Application.Interfaces;

public interface ICouponService
{
    Task<IEnumerable<CouponDto>> GetAllAsync();
    Task<CouponDto?> GetByIdAsync(int id);
    Task<CouponDto> CreateAsync(CreateCouponDto dto);
    Task<bool> UpdateAsync(UpdateCouponDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> ToggleActiveAsync(int id);
    Task<CouponValidationResult> ValidateAndCalculateAsync(
        string code,
        string userId,
        decimal orderSubtotal,
        bool isUserFirstOrder);
    Task RegisterUsageAsync(
        int couponId,
        string userId,
        int orderId,
        decimal discountApplied);
}