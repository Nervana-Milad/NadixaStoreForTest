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

    public class Coupon : BaseEntity
    {
        public string Code { get; set; } = string.Empty;  
        public CouponDiscountType DiscountType { get; set; }
        public decimal Value { get; set; }

        public decimal? MinOrderAmount { get; set; }      
        public decimal? MaxDiscountAmount { get; set; }  

        public int? MaxTotalUsage { get; set; }             
        public int? MaxUsagePerUser { get; set; } = 1;        

        public bool FirstOrderOnly { get; set; } = false;     

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<CouponUsage> Usages { get; set; } = new List<CouponUsage>();

        public bool IsCurrentlyValid =>
            IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    }


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
