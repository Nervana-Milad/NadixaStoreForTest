using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nadixa.Application.DTOS;

namespace Nadixa.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetStatsAsync();
    }
}
