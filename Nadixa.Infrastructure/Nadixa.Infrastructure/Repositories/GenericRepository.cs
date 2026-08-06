using Nadixa.Core.Common;
using Nadixa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore; // <-- مهم
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Nadixa.Core.Interfaces;

namespace Nadixa.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly NadixaDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(NadixaDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        private IQueryable<T> ApplyIncludes(IQueryable<T> query, Expression<Func<T, object>>[] includes)
        {
            foreach (var include in includes)
                query = query.Include(include);
            return query;
        }

        public async Task<T?> GetByIdAsync(int id, params Expression<Func<T, object>>[] includes)
        {
            var query = ApplyIncludes(_dbSet.AsQueryable(), includes);
            return await query.FirstOrDefaultAsync(e => e.Id == id);

        }

        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            var query = ApplyIncludes(_dbSet.AsNoTracking(), includes);
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes)
        {
            var query = ApplyIncludes(_dbSet.AsNoTracking(), includes);
            return await query.Where(predicate).ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, bool includeSoftDeleted = false)
        {
            var query = includeSoftDeleted
                ? _dbSet.IgnoreQueryFilters().AsQueryable()
                : _dbSet.AsQueryable();

            return await query.AnyAsync(predicate);
        }
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Delete(T entity)
        {
            entity.IsDeleted = true;
            _dbSet.Update(entity);
        }

        public void HardDelete(T entity)
        {
            _dbSet.Remove(entity);
        }
    }
}
