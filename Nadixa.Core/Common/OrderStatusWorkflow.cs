using Nadixa.Core.Entities;

namespace Nadixa.Core.Common
{
    public static class OrderStatusWorkflow
    {
        public static List<OrderStatus> GetAvailableStatuses(OrderStatus currentStatus)
        {
            return currentStatus switch
            {
                OrderStatus.Pending => new()
                {
                    OrderStatus.Pending,
                    OrderStatus.Confirmed,
                    OrderStatus.Cancelled
                },

                OrderStatus.Confirmed => new()
                {
                    OrderStatus.Confirmed,
                    OrderStatus.Processing,
                    OrderStatus.Cancelled
                },

                OrderStatus.Processing => new()
                {
                    OrderStatus.Processing,
                    OrderStatus.Shipped
                },

                OrderStatus.Shipped => new()
                {
                    OrderStatus.Shipped,
                    OrderStatus.OutForDelivery
                },

                OrderStatus.OutForDelivery => new()
                {
                    OrderStatus.OutForDelivery,
                    OrderStatus.Delivered
                },

                _ => new() { currentStatus }
            };
        }

        public static bool IsFinalStatus(OrderStatus status) =>
            status == OrderStatus.Delivered || status == OrderStatus.Cancelled;
    }
}
