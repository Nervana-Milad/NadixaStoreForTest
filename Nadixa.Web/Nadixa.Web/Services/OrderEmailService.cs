using Microsoft.AspNetCore.Identity;
using Nadixa.Core.Entities;
using Nadixa.Web.Services;
namespace Nadixa.Web.Services
{
    public class OrderEmailService
    {
        private readonly EmailSender _emailSender;
        private readonly UserManager<AppUser> _userManager;

        public OrderEmailService(
            EmailSender emailSender,
            UserManager<AppUser> userManager)
        {
            _emailSender = emailSender;
            _userManager = userManager;
        }


        public async Task SendOrderStatusEmailAsync(Order order)
        {
            var user = await _userManager.FindByIdAsync(order.UserId);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
                return;

            var (subject, title, message) = BuildEmailContent(order);

            var body = BuildTemplate(order, title, message);

            _emailSender.SendEmail(
                "Nadixa",
                "YOUR_EMAIL@gmail.com",   // الإيميل اللي هيبعت
                order.FullName,
                user.Email,
                subject,
                body);
        }
        private string BuildTemplate(Order order, string title, string message)
        {
            return $@"
    <!DOCTYPE html>
    <html>
    <body>
        <h2>{title}</h2>

        <p>Hello <b>{order.FullName}</b>,</p>

        <p>{message}</p>

        <hr/>

        <p><strong>Order Number:</strong> #{order.Id}</p>
        <p><strong>Status:</strong> {order.Status}</p>
        <p><strong>Total:</strong> {order.TotalPrice:C}</p>

        <a href='https://localhost:5001/Orders/Details/{order.Id}'>
            View Order
        </a>

        <br/><br/>

        <p>Thank you for shopping with Nadixa ❤️</p>
    </body>
    </html>";
        }

        private (string Subject, string Title, string Message) BuildEmailContent(Order order)
        {
            return order.Status switch
            {
                OrderStatus.Pending => (
                    $"Order #{order.Id} Received",
                    "Order Received",
                    "We've successfully received your order and will review it shortly."
                ),

                OrderStatus.Confirmed => (
                    $"Order #{order.Id} Confirmed",
                    "Order Confirmed",
                    "Your order has been confirmed and will be prepared soon."
                ),

                OrderStatus.Processing => (
                    $"Order #{order.Id} Processing",
                    "Preparing Your Order",
                    "Our team is currently preparing your order."
                ),

                OrderStatus.Shipped => (
                    $"Order #{order.Id} Shipped",
                    "Order Shipped",
                    "Great news! Your order has been shipped."
                ),

                OrderStatus.OutForDelivery => (
                    $"Order #{order.Id} Out for Delivery",
                    "Out For Delivery",
                    "Your order is on the way and will arrive soon."
                ),

                OrderStatus.Delivered => (
                    $"Order #{order.Id} Delivered",
                    "Order Delivered",
                    "Your order has been delivered successfully. Thank you for shopping with Nadixa."
                ),

                OrderStatus.Cancelled => (
                    $"Order #{order.Id} Cancelled",
                    "Order Cancelled",
                    "Unfortunately, your order has been cancelled."
                ),

                _ => (
                    "Order Updated",
                    "Order Updated",
                    "Your order status has changed."
                )
            };
        }
    }
}