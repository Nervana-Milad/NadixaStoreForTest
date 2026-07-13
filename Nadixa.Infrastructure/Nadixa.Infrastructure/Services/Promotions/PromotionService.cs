using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nadixa.Core.Services
{
    public class PromotionService : IPromotionService
    {
        // غيّري ApplicationDbContext لاسم الـ DbContext الفعلي عندك لو مختلف
        private readonly NadixaDbContext _context;

        public PromotionService(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Promotion>> GetActivePromotionsAsync()
        {
            var now = DateTime.Now;
            return await _context.Promotions
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .OrderByDescending(p => p.Priority)
                .ToListAsync();
        }

        public async Task<Promotion?> GetBestPromotionForProductAsync(int productId, int categoryId, int subCategoryId)
        {
            var active = await GetActivePromotionsAsync();

            return active
                .Where(p =>
                    p.Scope == PromotionScope.AllProducts ||
                    (p.Scope == PromotionScope.Category && p.ProductCategoryId == categoryId) ||
                    (p.Scope == PromotionScope.SubCategory && p.ProductSubCategoryId == subCategoryId) ||
                    (p.Scope == PromotionScope.SpecificProduct && p.ProductId == productId))
                .OrderByDescending(p => p.Priority)
                .FirstOrDefault();
        }

        public decimal CalculateDiscountedPrice(decimal originalPrice, Promotion promotion)
        {
            return promotion.Type switch
            {
                PromotionType.PercentageOff => Math.Round(originalPrice * (1 - (promotion.Value ?? 0) / 100), 2),
                PromotionType.FixedAmountOff => Math.Max(0, originalPrice - (promotion.Value ?? 0)),
                _ => originalPrice // BuyXGetYFree بتتحسب على مستوى السلة مش السعر الفردي
            };
        }

        public async Task<List<Promotion>> GetAllAsync()
        {
            return await _context.Promotions
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductSubCategory)
                .Include(p => p.Product)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task<Promotion?> GetByIdAsync(int id)
        {
            return await _context.Promotions.FindAsync(id);
        }

        public async Task<Promotion> CreateAsync(Promotion promotion)
        {
            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        public async Task<bool> UpdateAsync(Promotion promotion)
        {
            _context.Promotions.Update(promotion);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return false;

            _context.Promotions.Remove(promo);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var promo = await _context.Promotions.FindAsync(id);
            if (promo == null) return false;

            promo.IsActive = !promo.IsActive;
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
