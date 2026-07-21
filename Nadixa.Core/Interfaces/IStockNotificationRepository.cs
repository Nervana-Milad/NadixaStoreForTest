using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IStockNotificationRepository
    {
        Task<HashSet<int>> GetPendingProductIdsAsync(string userId);

    }
}
