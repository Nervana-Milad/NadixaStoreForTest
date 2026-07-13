using Nadixa.Core.DTOs;
using Nadixa.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IBundleDealService
    {
        // يفحص السلة ويرجع أي Bundle Deal اتحقق شرطها (كل منتجاتها موجودة في السلة) مع قيمة الخصم
        Task<List<(BundleDeal bundle, decimal discountAmount)>> GetMatchingBundlesAsync(List<CartLineItem> items);

        // لوحة تحكم الأدمن
        Task<List<BundleDeal>> GetAllAsync();
        Task<BundleDeal> CreateAsync(BundleDeal bundle);
        Task<bool> UpdateAsync(BundleDeal bundle);
        Task<bool> DeleteAsync(int id);
    }
}
