using Microsoft.EntityFrameworkCore;
using Nadixa.Core.DTOS;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Nadixa.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly NadixaDbContext _context;

        public ProductRepository(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync(int? categoryId)
        {
            var query = _context.Products.Include(p => p.ProductCategory).AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.ProductCategoryId == categoryId);

            return await query.ToListAsync();
        }

        public async Task<List<Product>> GetBestSellersAsync(int count)
        {
            return await _context.OrderItems
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.ProductCategory)
                .GroupBy(oi => oi.ProductId)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Take(count)
                .Select(g => g.First().Product)
                .ToListAsync();
        }

        public async Task<List<ProductSearchItem>> SearchAsync(string term, int take)
        {
            return await _context.Products
                .Where(p => p.Name.Contains(term) || p.Description.Contains(term))
                .Take(take)
                .Select(p => new ProductSearchItem
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.MainImageUrlPath,
                    Url = "/Product/Detail/" + p.Id
                })
                .ToListAsync();
        }


        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductSubCategory)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);
        }



        public async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
                            int? categoryId,
                            int? subCategoryId,
                            string? search,
                            int page,
                            int pageSize)
        {
            var query = _context.Products
                .Include(p => p.ProductCategory)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductCategoryId == categoryId.Value);
            }

            if (subCategoryId.HasValue)
            {
                query = query.Where(p =>
                    p.ProductSubCategoryId == subCategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            var total = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
