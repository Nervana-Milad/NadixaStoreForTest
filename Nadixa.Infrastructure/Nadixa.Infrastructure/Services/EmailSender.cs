//using MimeKit;
//using MailKit;
//using MailKit.Net.Smtp;
//using Microsoft.Extensions.Configuration;

//namespace Nadixa.Infrastructure.Services
//{
//    public class EmailSender
//    {
//        private readonly string smtpServer;
//        private readonly int smtpPort;
//        private readonly string smtpUsername;
//        private readonly string smtpPassword;


//        public EmailSender(IConfiguration configration)
//        {
//            smtpServer = configration.GetValue<string>("SmtpSettings:SmtpServer", "");
//            smtpPort = configration.GetValue<int>("SmtpSettings:SmtpPort", 0);
//            smtpUsername = configration.GetValue<string>("SmtpSettings:SmtpUsername", "");
//            smtpPassword = configration.GetValue<string>("SmtpSettings:SmtpPassword", "");
//        }

//        public bool SendEmail(string senderName, string senderEmail, string toName, string toEmail, string subject, string textContent) 
//        {
//            Console.WriteLine($"DEBUG: Attempting to send email to {toEmail}"); // 👈 ضيفي السطر ده

//            try
//            {
//                var message = new MimeMessage();
//                message.From.Add(new MailboxAddress(senderName, senderEmail));
//                message.To.Add(new MailboxAddress(toName, toEmail));
//                message.Subject = subject;

//                message.Body = new TextPart("html")
//                {
//                    Text = textContent
//                };

//                using (var client = new SmtpClient())
//                {
//                    client.Connect(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

//                    //Note: only needed if the SMTP server requires authentication
//                    client.Authenticate(smtpUsername, smtpPassword);
//                    client.Send(message);
//                    client.Disconnect(true);

//                }
//                Console.WriteLine("DEBUG: Email sent successfully"); // 👈 وده كمان

//                return true;

//            }
//            catch(Exception ex)
//            {
//                throw new Exception("EMAIL REAL ERROR: " + ex.ToString());

//                //Console.WriteLine("Email Sender Failure \n" + ex.ToString());

//                //return false;
//            }

//        }
//    }
//}

using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Nadixa.Infrastructure.Services
{
    public class EmailSender
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _logger = logger;

            _smtpServer = configuration["SmtpSettings:SmtpServer"] ?? "";
            _smtpPort = configuration.GetValue<int>("SmtpSettings:SmtpPort", 587);
            _smtpUsername = configuration["SmtpSettings:SmtpUsername"] ?? "";
            _smtpPassword = configuration["SmtpSettings:SmtpPassword"] ?? "";

            _logger.LogInformation(
                "EmailSender initialized. Server={Server}, Port={Port}, Username={UsernameSet}, PasswordLength={PasswordLength}",
                _smtpServer,
                _smtpPort,
                string.IsNullOrEmpty(_smtpUsername) ? "MISSING" : _smtpUsername,
                _smtpPassword?.Length ?? 0);
        }

        public bool SendEmail(
            string senderName,
            string senderEmail,
            string? toName,
            string? toEmail,
            string subject,
            string textContent)
        {
            if (string.IsNullOrWhiteSpace(_smtpServer) ||
                string.IsNullOrWhiteSpace(_smtpUsername) ||
                string.IsNullOrWhiteSpace(_smtpPassword))
            {
                _logger.LogError("SMTP settings are missing or empty. Check appsettings.json under the STARTUP project.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogError("Recipient email (toEmail) is null or empty.");
                return false;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(toName ?? toEmail, toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = textContent };

                using var client = new SmtpClient();
                client.Connect(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                client.Authenticate(_smtpUsername, _smtpPassword);
                client.Send(message);
                client.Disconnect(true);

                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EMAIL SEND FAILED for {ToEmail}", toEmail);
                return false;
            }
        }
    }
}