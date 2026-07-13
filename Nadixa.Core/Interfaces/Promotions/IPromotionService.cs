using Nadixa.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface IPromotionService
    {
        // للفرونت: كل العروض الشغالة دلوقتي
        Task<List<Promotion>> GetActivePromotionsAsync();

        // أفضل عرض ينطبق على منتج معين (بناءً على الأولوية Priority)
        Task<Promotion?> GetBestPromotionForProductAsync(int productId, int categoryId, int subCategoryId);

        // يحسب السعر بعد الخصم لعرضه في صفحة المنتج/الكارت
        decimal CalculateDiscountedPrice(decimal originalPrice, Promotion promotion);

        // لوحة تحكم الأدمن
        Task<List<Promotion>> GetAllAsync();
        Task<Promotion?> GetByIdAsync(int id);
        Task<Promotion> CreateAsync(Promotion promotion);
        Task<bool> UpdateAsync(Promotion promotion);
        Task<bool> DeleteAsync(int id);
        Task<bool> ToggleActiveAsync(int id);
    }
}
