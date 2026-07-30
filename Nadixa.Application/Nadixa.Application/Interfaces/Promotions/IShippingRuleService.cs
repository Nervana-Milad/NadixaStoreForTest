using Nadixa.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IShippingRuleService
    {
        // بيرجع (رسوم الشحن النهائية، قيمة الخصم على الشحن) بناءً على إجمالي الأوردر
        Task<(decimal finalShippingFee, decimal discountAmount, ShippingRule? appliedRule)>
            CalculateShippingAsync(decimal baseShippingFee, decimal orderSubtotal);

        // لوحة تحكم الأدمن
        Task<List<ShippingRule>> GetAllAsync();
        Task<ShippingRule> CreateAsync(ShippingRule rule);
        Task<bool> UpdateAsync(ShippingRule rule);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
    }
}
