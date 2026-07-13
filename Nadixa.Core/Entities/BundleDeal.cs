using Nadixa.Core.Common;
using System;
using System.Collections.Generic;

namespace Nadixa.Core.Entities
{
    /// <summary>
    /// عرض على مجموعة منتجات محددة تُشترى مع بعض بسعر خاص
    /// (مثال: شيّل + كريم = 250 جنيه بدل 320).
    /// </summary>
    public class BundleDeal : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public decimal BundlePrice { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<BundleDealProduct> Products { get; set; } = new List<BundleDealProduct>();

        public bool IsCurrentlyValid =>
            IsActive && DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    }

    public class BundleDealProduct
    {
        public int Id { get; set; }
        public int BundleDealId { get; set; }
        public BundleDeal BundleDeal { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
