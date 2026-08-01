// CouponDto.cs

namespace Nadixa.Application.DTOS.Coupons;

public class CouponDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string DiscountType { get; set; } = string.Empty;

    public decimal Value { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public int? MaxTotalUsage { get; set; }

    public int? MaxUsagePerUser { get; set; }

    public bool FirstOrderOnly { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; }
}
