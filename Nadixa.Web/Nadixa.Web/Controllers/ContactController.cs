using Microsoft.AspNetCore.Mvc;
using Nadixa.Infrastructure.Services;
using Nadixa.Web.Helpers;

namespace Nadixa.Web.Controllers
{
    public class ContactController : Controller
    {
        private readonly EmailSender _emailSender;

        public ContactController(EmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string email, string msg)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(msg))
            {
                TempData["Error"] = AppMessages.ContactFieldsRequired;
                return RedirectToAction("Index");
            }

            var isSent = _emailSender.SendEmail(
                senderName: "NadixaStore Contact",
                senderEmail: "vanamilad2@gmail.com",   // 👈 لازم يكون نفس SmtpUsername بتاعك
                toName: "Admin",
                toEmail: "vanamilad2@gmail.com",       // 👈 إيميل الأدمن الثابت اللي بيستقبل الرسائل
                subject: "New Contact Message",
                textContent: $"You have received a new message from {email}:\n\n{msg}"
            );

            if (isSent)
            {
                TempData["Success"] = AppMessages.ContactMessageSent;
            }
            else
            {
                TempData["Error"] = AppMessages.ContactMessageFailed;
            }

            return RedirectToAction("Index");
        }
    }
}