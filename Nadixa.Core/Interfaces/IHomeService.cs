using Nadixa.Core.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IHomeService
    {
        Task<HomeIndexResult> GetIndexDataAsync(int? categoryId, string? userId);
        Task<GlobalSearchResult> GlobalSearchAsync(string term);

    }
}
