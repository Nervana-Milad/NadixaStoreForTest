using System.Threading.Tasks;

namespace Nadixa.Core.Interfaces
{
    public interface ILoyaltyService
    {
        Task<int> GetBalanceAsync(string userId);

        // كام نقطة هيكسبها العميل لو الأوردر ده قيمته كذا (بدون تسجيل فعلي، للعرض بس)
        int PreviewPointsToEarn(decimal orderTotal);

        // تسجيل النقاط فعليًا بعد تأكيد الأوردر
        Task AddPointsForOrderAsync(string userId, int orderId, decimal orderTotal);

        // تحويل عدد نقاط لخصم بالجنيه (بدون خصمها من الرصيد - للعرض/الحساب المبدئي)
        decimal ConvertPointsToDiscount(int points);

        // خصم النقاط من الرصيد فعليًا بعد تأكيد استخدامها في أوردر
        Task<bool> RedeemPointsAsync(string userId, int points, int orderId);
    }
}
