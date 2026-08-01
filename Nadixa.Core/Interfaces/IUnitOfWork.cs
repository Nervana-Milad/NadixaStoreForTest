using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> Repository<T>() where T : Common.BaseEntity;
        ICouponRepository Coupons { get; }
        IDashboardRepository Dashboard { get; }
        IUserRepository Users { get; }
        Task<int> CompleteAsync();
    }
}
