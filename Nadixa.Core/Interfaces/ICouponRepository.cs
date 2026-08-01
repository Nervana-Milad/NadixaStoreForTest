using Nadixa.Core.Entities;

namespace Nadixa.Core.Interfaces;

public interface ICouponRepository : IGenericRepository<Coupon>
{
    Task<Coupon?> GetByCodeAsync(string code);

    Task<int> GetTotalUsageCountAsync(int couponId);

    Task<int> GetUserUsageCountAsync(
        int couponId,
        string userId);
}

