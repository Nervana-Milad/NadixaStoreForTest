using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface ICartRepository
    {
        Task<Dictionary<int, int>> GetCartItemsAsync(string userId);

    }
}
