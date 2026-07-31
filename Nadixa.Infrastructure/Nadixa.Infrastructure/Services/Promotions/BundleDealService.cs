using Microsoft.EntityFrameworkCore;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Entities;
using Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class BundleDealService :IBundleDealService
    {
        private readonly NadixaDbContext _context;

        public BundleDealService(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<List<(BundleDeal bundle, decimal discountAmount)>> GetMatchingBundlesAsync(List<CartLineItem> items)
        {
            var now = DateTime.Now;
            var activeBundles = await _context.BundleDeals
                .Include(b => b.Products)
                .Where(b => b.IsActive && b.StartDate <= now && b.EndDate >= now)
                .ToListAsync();

            var cartProductIds = items.Select(i => i.ProductId).ToHashSet();
            var result = new List<(BundleDeal, decimal)>();

            foreach (var bundle in activeBundles)
            {
                var bundleProductIds = bundle.Products.Select(p => p.ProductId).ToList();

                // شرط التفعيل: كل منتجات الباندل موجودة في السلة
                if (bundleProductIds.All(id => cartProductIds.Contains(id)))
                {
                    var originalTotal = items
                        .Where(i => bundleProductIds.Contains(i.ProductId))
                        .Sum(i => i.UnitPrice);

                    var discount = Math.Max(0, originalTotal - bundle.BundlePrice);
                    if (discount > 0)
                        result.Add((bundle, discount));
                }
            }

            return result;
        }

        public async Task<List<BundleDeal>> GetAllAsync()
        {
            return await _context.BundleDeals
                .Include(b => b.Products)
                    .ThenInclude(p => p.Product)
                .OrderByDescending(b => b.Id)
                .ToListAsync();
        }

        public async Task<BundleDeal> CreateAsync(BundleDeal bundle)
        {
            _context.BundleDeals.Add(bundle);
            await _context.SaveChangesAsync();
            return bundle;
        }

        public async Task<bool> UpdateAsync(BundleDeal bundle)
        {
            _context.BundleDeals.Update(bundle);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bundle = await _context.BundleDeals.FindAsync(id);
            if (bundle == null) return false;

            _context.BundleDeals.Remove(bundle);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
