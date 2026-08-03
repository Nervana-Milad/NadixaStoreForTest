using Nadixa.Core.Common;
using Nadixa.Core.Entities;
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
    public IOrderRepository Orders { get; }
    public ICartRepository Carts { get; }
    public IWishlistRepository Wishlists { get; }

    public UnitOfWork(NadixaDbContext context,
        ICouponRepository couponRepository,
        IDashboardRepository dashboardRepository,
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        ICartRepository cartRepository,
        IWishlistRepository wishlistRepository)
    {
        _context = context;
        Coupons = couponRepository;
        Dashboard = dashboardRepository;
        Users = userRepository;
        Orders = orderRepository;
        Carts = cartRepository;
        Wishlists = wishlistRepository;
    }
    public IGenericRepository<OrderStatusHistory> OrderStatusHistories => Repository<OrderStatusHistory>();

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
