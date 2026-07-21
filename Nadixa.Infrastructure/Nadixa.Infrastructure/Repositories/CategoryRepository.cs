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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly NadixaDbContext _context;

        public CategoryRepository(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductCategory>> GetAllAsync()
        {
            return await _context.ProductCategories.ToListAsync();
        }

        public async Task<List<CategorySearchItem>> SearchAsync(string term, int take)
        {
            return await _context.ProductCategories
                .Where(c => c.Name.Contains(term))
                .Take(take)
                .Select(c => new CategorySearchItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Url = "/Product/Index?categoryId=" + c.Id
                })
                .ToListAsync();
        }
    }
}
