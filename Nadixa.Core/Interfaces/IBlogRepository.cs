using Nadixa.Core.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IBlogRepository
    {
        Task<List<BlogSearchItem>> SearchAsync(string term, int take);
    }
}
