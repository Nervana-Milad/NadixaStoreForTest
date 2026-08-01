using Nadixa.Core.Entities;

namespace Nadixa.Core.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetOrderWithItemsAsync(int id);
    }
}