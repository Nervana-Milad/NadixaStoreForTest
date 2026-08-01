using Nadixa.Core.Common;
using System;
using System.Collections.Generic;

namespace Nadixa.Core.Entities
{
    public enum CouponDiscountType
    {
        Percentage,
        FixedAmount
    }

    /// <summary>
    /// كود خصم يدخله العميل بنفسه وقت الـ Checkout.
    /// </summary>
    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = string.Empty;   // "WELCOME10"
        public CouponDiscountType DiscountType { get; set; }
        public decimal Value { get; set; }

        public decimal? MinOrderAmount { get; set; }        // أقل قيمة أوردر عشان الكود يشتغل
        public decimal? MaxDiscountAmount { get; set; }      // سقف الخصم (مفيد لو النوع نسبة %)

        public int? MaxTotalUsage { get; set; }               // إجمالي مرات الاستخدام لكل العملاء
        public int? MaxUsagePerUser { get; set; } = 1;         // كام مرة اليوزر الواحد يقدر يستخدمه

        public bool FirstOrderOnly { get; set; } = false;      // أول أوردر بس

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();

        public bool IsCurrentlyValid =>
            IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    }

    /// <summary>
    /// سجل استخدام لكل مرة يتم فيها تطبيق الكوبون على أوردر - يمنع تكرار الاستخدام
    /// أكتر من المسموح ويحتفظ بالخصم الفعلي اللي اتطبق وقتها.
    /// </summary>
    public class CouponUsage : BaseEntity
    {
        public int CouponId { get; set; }
        public Coupon Coupon { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public decimal DiscountApplied { get; set; }
        public DateTime? UsedAt { get; set; } = DateTime.Now;
    }
}
