using Microsoft.EntityFrameworkCore;
using Nadixa.Application.DTOS;
using Nadixa.Application.Interfaces;
using Nadixa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class StockNotificationService
    {
        private readonly NadixaDbContext _context;
        private readonly EmailSender _emailSender;
        private readonly IRazorViewRenderer _viewRenderer;

        public StockNotificationService(
            NadixaDbContext context,
            EmailSender emailSender,
            IRazorViewRenderer viewRenderer)
        {
            _context = context;
            _emailSender = emailSender;
            _viewRenderer = viewRenderer;
        }

        public async Task NotifySubscribersAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.StockQuantity <= 0)
                return;

            var pendingRequests = await _context.StockNotificationRequests
                .Include(r => r.User)
                .Where(r => r.ProductId == productId && !r.IsNotified)
                .ToListAsync();

            if (!pendingRequests.Any())
                return;

            foreach (var request in pendingRequests)
            {
                try
                {
                    var emailModel = new NotifyMeWhenProdRestockDto
                    {
                        CustomerName = request.User.FirstName ?? "Customer",
                        ProductName = product.Name,
                        ProductImageUrl = product.MainImageUrlPath,
                        Price = product.Price,
                        ProductUrl = $"https://yourstore.com/Product/Detail/{product.Id}"
                    };

                    var emailBody = await _viewRenderer.RenderAsync(
                        "Emails/NotifyMeWhenProdRestock", emailModel);

                    _emailSender.SendEmail(
                        senderName: "Nadixa Store",
                        senderEmail: "vanamilad2@gmail.com",
                        toName: request.User.FirstName ?? "Customer",
                        toEmail: request.User.Email,
                        subject: $"'{product.Name}' is back in stock!",
                        textContent: emailBody
                    );

                    request.IsNotified = true;
                }
                catch (Exception ex)
                {
                    throw new Exception("NOTIFY REAL ERROR: " + ex.ToString());
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task NotifySubscribersForMultipleAsync(IEnumerable<int> productIds)
        {
            foreach (var id in productIds)
            {
                await NotifySubscribersAsync(id);
            }
        }

    }
}
