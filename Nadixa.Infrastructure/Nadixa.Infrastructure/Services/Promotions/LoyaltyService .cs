using Microsoft.EntityFrameworkCore;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services

{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly NadixaDbContext _context;

        // إعدادات قابلة للتعديل حسب سياسة المتجر
        private const decimal EgpPerPointEarned = 10m;   // كل 10 جنيه إنفاق = نقطة
        private const decimal EgpValuePerPointRedeemed = 0.10m; // كل نقطة = 10 قروش خصم

        public LoyaltyService(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetBalanceAsync(string userId)
        {
            var account = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(a => a.UserId == userId);
            return account?.PointsBalance ?? 0;
        }

        public int PreviewPointsToEarn(decimal orderTotal)
        {
            return (int)Math.Floor(orderTotal / EgpPerPointEarned);
        }

        public decimal ConvertPointsToDiscount(int points)
        {
            return Math.Round(points * EgpValuePerPointRedeemed, 2);
        }

        public async Task AddPointsForOrderAsync(string userId, int orderId, decimal orderTotal)
        {
            var points = PreviewPointsToEarn(orderTotal);
            if (points <= 0) return;

            var account = await GetOrCreateAccountAsync(userId);
            account.PointsBalance += points;

            _context.LoyaltyTransactions.Add(new LoyaltyTransaction
            {
                LoyaltyAccountId = account.Id,
                Type = LoyaltyTransactionType.Earned,
                Points = points,
                OrderId = orderId,
                Note = "نقاط مكتسبة من الأوردر"
            });

            await _context.SaveChangesAsync();
        }

        public async Task<bool> RedeemPointsAsync(string userId, int points, int orderId)
        {
            var account = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null || account.PointsBalance < points || points <= 0)
                return false;

            account.PointsBalance -= points;

            _context.LoyaltyTransactions.Add(new LoyaltyTransaction
            {
                LoyaltyAccountId = account.Id,
                Type = LoyaltyTransactionType.Redeemed,
                Points = -points,
                OrderId = orderId,
                Note = "استبدال نقاط كخصم على الأوردر"
            });

            return await _context.SaveChangesAsync() > 0;
        }

        private async Task<LoyaltyAccount> GetOrCreateAccountAsync(string userId)
        {
            var account = await _context.LoyaltyAccounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account != null) return account;

            account = new LoyaltyAccount { UserId = userId, PointsBalance = 0 };
            _context.LoyaltyAccounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }
    }
}
