using Nadixa.Core.Common;
using System;
using System.Collections.Generic;

namespace Nadixa.Core.Entities
{
    // نوع الخصم
    public enum PromotionType
    {
        PercentageOff,      // خصم نسبة % (مثال: خصم 20%)
        FixedAmountOff,     // خصم مبلغ ثابت (مثال: خصم 100 جنيه)
        BuyXGetYFree        // اشتري X خد Y ببلاش (مثال: 2 باي 1 فري)
    }

    // نطاق تطبيق العرض
    public enum PromotionScope
    {
        AllProducts,
        Category,
        SubCategory,
        SpecificProduct
    }

    /// <summary>
    /// حملة/عرض ترويجي. تغطي الخصومات على المنتجات (نسبة/مبلغ ثابت/BuyXGetY)،
    /// وتُستخدم أيضًا لتمثيل الـ Flash Sale (بمجرد تحديد فترة زمنية قصيرة)
    /// وخصم أول عملية شراء (IsFirstPurchaseOnly).
    /// </summary>
    public class Promotion : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // للعرض على الفرونت (بادچ فوق كارت المنتج)
        public string? BadgeText { get; set; }        // "خصم 20%" / "عرض لفترة محدودة"
        public string? BadgeColorHex { get; set; }     // "#FF3B30"

        public PromotionType Type { get; set; }
        public PromotionScope Scope { get; set; }

        // تُستخدم مع PercentageOff / FixedAmountOff
        public decimal? Value { get; set; }

        // تُستخدم مع BuyXGetYFree (مثال: Buy=2, Free=1 => 2 باي 1 فري)
        public int? BuyQuantity { get; set; }
        public int? FreeQuantity { get; set; }

        // نطاق التطبيق (حسب الـ Scope يتم استخدام واحد منهم فقط)
        public int? ProductCategoryId { get; set; }
        public ProductCategory? ProductCategory { get; set; }

        public int? ProductSubCategoryId { get; set; }
        public ProductSubCategory? ProductSubCategory { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // لو أكتر من عرض ينطبق على نفس المنتج، الأعلى Priority هو اللي بيتحسب
        public int Priority { get; set; } = 0;

        // خصم أول عملية شراء فقط (بيتفحص وقت الحساب هل اليوزر عنده أوردرات قبل كده)
        public bool IsFirstPurchaseOnly { get; set; } = false;

        // هل العرض شغال دلوقتي فعليًا (تاريخ + حالة التفعيل)
        public bool IsCurrentlyValid =>
            IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate;

        // مفيدة لعرض عداد تنازلي (Flash Sale) في الفرونت
        public bool IsFlashSale => IsCurrentlyValid && (EndDate - StartDate).TotalHours <= 72;
    }
}
