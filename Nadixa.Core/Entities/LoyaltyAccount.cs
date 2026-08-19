using Nadixa.Core.Common;
using System;
using System.Collections.Generic;

namespace Nadixa.Core.Entities
{
    public enum LoyaltyTransactionType
    {
        Earned,
        Redeemed,
        Expired
    }

    public class LoyaltyAccount : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int PointsBalance { get; set; }

        public ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
    }

    public class LoyaltyTransaction : BaseEntity
    {
        public int LoyaltyAccountId { get; set; }
        public LoyaltyAccount LoyaltyAccount { get; set; } = null!;

        public LoyaltyTransactionType Type { get; set; }
        public int Points { get; set; }
        public int? OrderId { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
