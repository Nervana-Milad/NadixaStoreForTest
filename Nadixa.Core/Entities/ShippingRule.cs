using Nadixa.Core.Common;

namespace Nadixa.Core.Entities
{
    public enum ShippingDiscountType
    {
        FreeShipping,     // شحن ببلاش
        PercentageOff     // خصم نسبة من الشحن
    }

    /// <summary>
    /// قاعدة شحن حسب إجمالي الأوردر. ممكن يكون عندك أكتر من قاعدة
    /// (مثال: خصم 50% فوق 1000، وشحن ببلاش فوق 2000) والأعلى Priority
    /// اللي بيتطبق لو الأوردر بيحقق أكتر من شرط في نفس الوقت.
    /// </summary>
    public class ShippingRule : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal MinOrderAmount { get; set; }
        public ShippingDiscountType DiscountType { get; set; }

        // تُستخدم لو DiscountType = PercentageOff (مثال: 20 يعني خصم 20% من الشحن)
        public decimal? PercentageValue { get; set; }

        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 0;
    }
}
