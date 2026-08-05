using Nadixa.Application.DTOS;
using Nadixa.Application.DTOS.Promotion;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    /// <summary>
    /// نقطة الدخول الوحيدة لحساب السعر النهائي للسلة/الأوردر.
    /// بتجمع كل أنواع الخصومات (عروض المنتجات، الباندل، الكوبون، الشحن، نقاط الولاء)
    /// في مكان واحد، عشان الكونترولرز متعرفش تفاصيل كل نوع خصم لوحده.
    /// </summary>
    public interface IPricingEngine
    {
        Task<CartPricingResult> CalculateAsync(CartPricingRequest request);
    }
}
