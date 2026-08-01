using Nadixa.Application.DTOS.Coupons;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;

namespace Nadixa.Infrastructure.Services;

public class CouponService : ICouponService
{
    private readonly IUnitOfWork _unitOfWork;

    public CouponService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // =========================================
    // GET ALL
    // =========================================
    public async Task<IEnumerable<CouponDto>> GetAllAsync()
    {
        var coupons = await _unitOfWork.Coupons.GetAllAsync();
        return coupons.Select(MapToDto);
    }

    // =========================================
    // GET BY ID
    // =========================================
    public async Task<CouponDto?> GetByIdAsync(int id)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);
        return coupon is null ? null : MapToDto(coupon);
    }

    // =========================================
    // CREATE
    // =========================================
    public async Task<CouponDto> CreateAsync(CreateCouponDto dto)
    {
        var coupon = new Coupon
        {
            Code = NormalizeCode(dto.Code),
            DiscountType = Enum.Parse<CouponDiscountType>(dto.DiscountType, true),
            Value = dto.Value,
            MinOrderAmount = dto.MinOrderAmount,
            MaxDiscountAmount = dto.MaxDiscountAmount,
            MaxTotalUsage = dto.MaxTotalUsage,
            MaxUsagePerUser = dto.MaxUsagePerUser,
            FirstOrderOnly = dto.FirstOrderOnly,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate ?? DateTime.MaxValue,
            IsActive = dto.IsActive
        };

        await _unitOfWork.Coupons.AddAsync(coupon);
        await _unitOfWork.CompleteAsync();

        return MapToDto(coupon);
    }

    // =========================================
    // UPDATE
    // =========================================
    public async Task<bool> UpdateAsync(UpdateCouponDto dto)
    {
        var existingCoupon = await _unitOfWork.Coupons.GetByIdAsync(dto.Id);

        if (existingCoupon is null)
            return false;

        existingCoupon.Code = NormalizeCode(dto.Code);
        existingCoupon.DiscountType = Enum.Parse<CouponDiscountType>(dto.DiscountType, true);
        existingCoupon.Value = dto.Value;
        existingCoupon.MinOrderAmount = dto.MinOrderAmount;
        existingCoupon.MaxDiscountAmount = dto.MaxDiscountAmount;
        existingCoupon.MaxTotalUsage = dto.MaxTotalUsage;
        existingCoupon.MaxUsagePerUser = dto.MaxUsagePerUser;
        existingCoupon.FirstOrderOnly = dto.FirstOrderOnly;
        existingCoupon.StartDate = dto.StartDate;
        existingCoupon.EndDate = dto.EndDate ?? DateTime.MaxValue;
        existingCoupon.IsActive = dto.IsActive;

        _unitOfWork.Coupons.Update(existingCoupon);

        return await _unitOfWork.CompleteAsync() > 0;
    }

    // =========================================
    // DELETE
    // =========================================
    public async Task<bool> DeleteAsync(int id)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);

        if (coupon is null)
            return false;

        _unitOfWork.Coupons.Delete(coupon);

        return await _unitOfWork.CompleteAsync() > 0;
    }

    // =========================================
    // TOGGLE ACTIVE
    // =========================================
    public async Task<bool> ToggleActiveAsync(int id)
    {
        var coupon = await _unitOfWork.Coupons.GetByIdAsync(id);

        if (coupon is null)
            return false;

        coupon.IsActive = !coupon.IsActive;

        _unitOfWork.Coupons.Update(coupon);

        return await _unitOfWork.CompleteAsync() > 0;
    }

    // =========================================
    // VALIDATE & CALCULATE
    // =========================================
    public async Task<CouponValidationResult> ValidateAndCalculateAsync(
        string code,
        string userId,
        decimal orderSubtotal,
        bool isUserFirstOrder)
    {
        var coupon = await _unitOfWork.Coupons.GetByCodeAsync(code);

        if (coupon is null)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                DiscountAmount = 0,
                Error = "Coupon code does not exist.",
                Coupon = null
            };
        }

        if (!coupon.IsCurrentlyValid)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                DiscountAmount = 0,
                Error = "Coupon is expired or inactive.",
                Coupon = null
            };
        }

        if (coupon.FirstOrderOnly && !isUserFirstOrder)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                DiscountAmount = 0,
                Error = "This coupon is only valid for your first order.",
                Coupon = null
            };
        }

        if (coupon.MinOrderAmount.HasValue && orderSubtotal < coupon.MinOrderAmount.Value)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                DiscountAmount = 0,
                Error = $"The minimum order amount for this coupon is {coupon.MinOrderAmount.Value}.",
                Coupon = null
            };
        }

        var totalUsage = await _unitOfWork.Coupons.GetTotalUsageCountAsync(coupon.Id);

        if (coupon.MaxTotalUsage.HasValue && totalUsage >= coupon.MaxTotalUsage.Value)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                DiscountAmount = 0,
                Error = "This coupon has reached its maximum usage limit.",
                Coupon = null
            };
        }

        var userUsage = await _unitOfWork.Coupons.GetUserUsageCountAsync(coupon.Id, userId);

        if (coupon.MaxUsagePerUser.HasValue && userUsage >= coupon.MaxUsagePerUser.Value)
        {
            return new CouponValidationResult
            {
                IsValid = false,
                DiscountAmount = 0,
                Error = "You have already used this coupon.",
                Coupon = null
            };
        }

        var discount = CalculateDiscount(coupon, orderSubtotal);

        return new CouponValidationResult
        {
            IsValid = true,
            DiscountAmount = discount,
            Error = null,
            Coupon = coupon
        };
    }

    // =========================================
    // REGISTER USAGE
    // =========================================
    public async Task RegisterUsageAsync(
        int couponId,
        string userId,
        int orderId,
        decimal discountApplied)
    {
        var usage = new CouponUsage
        {
            CouponId = couponId,
            UserId = userId,
            OrderId = orderId,
            DiscountApplied = discountApplied,
            UsedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<CouponUsage>().AddAsync(usage);
        await _unitOfWork.CompleteAsync();
    }

    // =========================================
    // PRIVATE HELPERS
    // =========================================
    private static decimal CalculateDiscount(Coupon coupon, decimal orderSubtotal)
    {
        var discount = coupon.DiscountType == CouponDiscountType.Percentage
            ? orderSubtotal * (coupon.Value / 100)
            : coupon.Value;

        if (coupon.MaxDiscountAmount.HasValue)
            discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);

        discount = Math.Min(discount, orderSubtotal);

        return Math.Round(discount, 2);
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToUpperInvariant();
    }

    private static CouponDto MapToDto(Coupon coupon)
    {
        return new CouponDto
        {
            Id = coupon.Id,
            Code = coupon.Code,
            DiscountType = coupon.DiscountType.ToString(),
            Value = coupon.Value,
            MinOrderAmount = coupon.MinOrderAmount,
            MaxDiscountAmount = coupon.MaxDiscountAmount,
            MaxTotalUsage = coupon.MaxTotalUsage,
            MaxUsagePerUser = coupon.MaxUsagePerUser,
            FirstOrderOnly = coupon.FirstOrderOnly,
            StartDate = coupon.StartDate,
            EndDate = coupon.EndDate,
            IsActive = coupon.IsActive
        };
    }
}