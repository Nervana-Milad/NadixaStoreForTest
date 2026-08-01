using Nadixa.Core.Entities;

namespace Nadixa.Application.Interfaces
{
    public interface IOrderEmailService
    {
        Task SendOrderStatusEmailAsync(Order order);
    }
}
