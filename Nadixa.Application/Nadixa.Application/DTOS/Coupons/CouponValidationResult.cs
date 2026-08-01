using Nadixa.Core.Entities;

namespace Nadixa.Application.DTOS.Coupons;

public class CouponValidationResult
{
    public bool IsValid { get; init; }

    public decimal DiscountAmount { get; init; }

    public string? Error { get; init; }

    public Coupon? Coupon { get; init; }
}
