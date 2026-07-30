using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nadixa.Application.Interfaces;
using Nadixa.Infrastructure.Data;

namespace Nadixa.Infrastructure.Services

{
    public class ShippingRuleService : IShippingRuleService
    {
        private readonly NadixaDbContext _context;

        public ShippingRuleService(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<(decimal finalShippingFee, decimal discountAmount, ShippingRule? appliedRule)>
            CalculateShippingAsync(decimal baseShippingFee, decimal orderSubtotal)
        {
            var rule = await _context.ShippingRules
                .Where(r => r.IsActive && r.MinOrderAmount <= orderSubtotal)
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => r.MinOrderAmount)
                .FirstOrDefaultAsync();

            if (rule == null)
                return (baseShippingFee, 0, null);

            if (rule.DiscountType == ShippingDiscountType.FreeShipping)
                return (0, baseShippingFee, rule);

            var discount = baseShippingFee * ((rule.PercentageValue ?? 0) / 100);
            var finalFee = baseShippingFee - discount;
            return (finalFee, discount, rule);
        }

        public async Task<List<ShippingRule>> GetAllAsync()
        {
            return await _context.ShippingRules.OrderByDescending(r => r.Id).ToListAsync();
        }

        public async Task<ShippingRule> CreateAsync(ShippingRule rule)
        {
            _context.ShippingRules.Add(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task<bool> UpdateAsync(ShippingRule rule)
        {
            _context.ShippingRules.Update(rule);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rule = await _context.ShippingRules.FindAsync(id);
            if (rule == null) return false;

            _context.ShippingRules.Remove(rule);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var rule = await _context.ShippingRules.FindAsync(id);
            if (rule == null) return false;

            rule.IsActive = !rule.IsActive;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
