using Microsoft.AspNetCore.Mvc;
using Nadixa.Web.Services;

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
            if(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(msg))
            {
                ViewBag.Error = "Please fill all fields";
                return View();
            }


            var isSent = _emailSender.SendEmail(
                senderName: "NadixaStore Conatct",
                senderEmail: "nervanamilad143@gmail.com",
                toName: "Admin",
                toEmail: "nardinmilad83@gmail.com",
                subject: "New Contact Message",
                textContent: $"You have received a new message from {email}:\n\n{msg}"

                );

            if (isSent)
            {
                ViewBag.Success = "Your message has been sent successfully!";
            }
            else {
                ViewBag.Error = "Failed to send message. Please try again!";
            }

            return View();
        }
    }
}
