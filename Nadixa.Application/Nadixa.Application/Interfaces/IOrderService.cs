using Nadixa.Application.DTOS.order;
using Nadixa.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IOrderService
    {
        Task<List<AdminOrderDto>> GetAllOrdersForAdminAsync();
        Task<OrderDetailsDto?> GetOrderDetailsAsync(int id);
        Task<OrderStatusUpdateResult> UpdateOrderStatusAsync(UpdateOrderStatusDto dto);
    }

    // Interface بدل الاعتماد المباشر على OrderEmailService الملموسة
    // implementation بتاعها ينتقل لـ Nadixa.Infrastructure.Services
 
}
