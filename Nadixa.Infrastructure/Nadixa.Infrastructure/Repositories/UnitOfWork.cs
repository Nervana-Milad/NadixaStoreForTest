using Nadixa.Core.Common;
using Nadixa.Core.Interfaces;
using Nadixa.Infrastructure.Data;
using System.Collections;

namespace Nadixa.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly NadixaDbContext _context;
    private readonly Hashtable _repositories = new();

    public ICouponRepository Coupons { get; }

    public IDashboardRepository Dashboard { get; }

    public IUserRepository Users { get; }

    public UnitOfWork(
        NadixaDbContext context,
        ICouponRepository couponRepository,
        IDashboardRepository dashboardRepository,
        IUserRepository userRepository)
    {
        _context = context;

        Coupons = couponRepository;
        Dashboard = dashboardRepository;
        Users = userRepository;
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public IGenericRepository<T> Repository<T>()
        where T : BaseEntity
    {
        var type = typeof(T).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType =
                typeof(GenericRepository<>);

            var repositoryInstance =
                Activator.CreateInstance(
                    repositoryType.MakeGenericType(typeof(T)),
                    _context);

            _repositories.Add(
                type,
                repositoryInstance);
        }

        return (IGenericRepository<T>)
            _repositories[type]!;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
