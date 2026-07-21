using Microsoft.EntityFrameworkCore;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;

namespace Nadixa.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly NadixaDbContext _context;

        public ProductRepository(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
            int? categoryId, int? subCategoryId, string? search, int page, int pageSize)
        {
            IQueryable<Product> query = _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory);

            if (categoryId.HasValue)
                query = query.Where(p => p.ProductCategoryId == categoryId.Value);

            if (subCategoryId.HasValue)
                query = query.Where(p => p.ProductSubCategoryId == subCategoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));

            int total = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}