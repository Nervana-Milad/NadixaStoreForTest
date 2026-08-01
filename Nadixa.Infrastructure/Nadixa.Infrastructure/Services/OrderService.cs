using AutoMapper;
using Nadixa.Application.DTOS.order;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;

namespace Nadixa.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IOrderEmailService _orderEmailService;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOrderEmailService orderEmailService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _orderEmailService = orderEmailService;
        }

        public async Task<List<AdminOrderDto>> GetAllOrdersForAdminAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();

            return orders
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new AdminOrderDto
                {
                    Id = o.Id,
                    CustomerName = o.FullName,
                    CreatedAt = o.CreatedAt,
                    Status = o.Status,
                    AvailableStatuses = OrderStatusWorkflow.GetAvailableStatuses(o.Status),
                    GrandTotal = o.TotalPrice
                }).ToList();
        }

        public async Task<OrderDetailsDto?> GetOrderDetailsAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetOrderWithItemsAsync(id);
            if (order == null) return null;

            var dto = _mapper.Map<OrderDetailsDto>(order);
            dto.AvailableStatuses = OrderStatusWorkflow.GetAvailableStatuses(order.Status);

            return dto;
        }

        public async Task<OrderStatusUpdateResult> UpdateOrderStatusAsync(UpdateOrderStatusDto dto)
        {
            // GetByIdAsync من الـ GenericRepository بيرجع الـ entity متتبّعة (tracked)
            // زي ما كان بيحصل مع FindAsync بالظبط
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
            if (order == null)
                return OrderStatusUpdateResult.Fail("Order not found.");

            if (OrderStatusWorkflow.IsFinalStatus(order.Status))
                return OrderStatusUpdateResult.Fail("This order status can no longer be changed.");

            if (!Enum.TryParse<OrderStatus>(dto.Status, out var newStatus))
                return OrderStatusUpdateResult.Fail("Invalid status value.");

            var allowedStatuses = OrderStatusWorkflow.GetAvailableStatuses(order.Status);
            if (!allowedStatuses.Contains(newStatus))
                return OrderStatusUpdateResult.Fail("Invalid status transition.");

            order.Status = newStatus;
            _unitOfWork.Orders.Update(order);

            await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = newStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = dto.AdminUserName ?? "System"
            });

            await _unitOfWork.CompleteAsync();
            await _orderEmailService.SendOrderStatusEmailAsync(order);

            return OrderStatusUpdateResult.Ok();
        }
    }
}
