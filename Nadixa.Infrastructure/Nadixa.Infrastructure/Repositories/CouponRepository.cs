using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;

namespace Nadixa.Infrastructure.Repositories;

public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
{
    private readonly NadixaDbContext _context;

    public CouponRepository(NadixaDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<Coupon?> GetByCodeAsync(string code)
    {
        return await _context.Coupons
            .FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<int> GetTotalUsageCountAsync(int couponId)
    {
        return await _context.CouponUsages
            .CountAsync(u => u.CouponId == couponId);
    }

    public async Task<int> GetUserUsageCountAsync(
        int couponId,
        string userId)
    {
        return await _context.CouponUsages
            .CountAsync(u =>
                u.CouponId == couponId &&
                u.UserId == userId);
    }
}
