using Microsoft.EntityFrameworkCore;
using Nadixa.Application.Interfaces;
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
    public class BlogRepository : IBlogRepository
    {
        private readonly NadixaDbContext _context;

        public BlogRepository(NadixaDbContext context)
        {
            _context = context;
        }

        public async Task<List<BlogSearchItem>> SearchAsync(string term, int take)
        {
            return await _context.Blogs
                .Where(b => b.Title.Contains(term) || b.Content.Contains(term))
                .Take(take)
                .Select(b => new BlogSearchItem
                {
                    Id = b.Id,
                    Name = b.Title,
                    Url = "/Blog/Detail/" + b.Id
                })
                .ToListAsync();
        }

    }
}
