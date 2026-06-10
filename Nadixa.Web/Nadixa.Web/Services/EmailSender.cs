using MimeKit;
using MailKit;
using MailKit.Net.Smtp;

namespace Nadixa.Web.Services
{
    public class EmailSender
    {
        private readonly string smtpServer;
        private readonly int smtpPort;
        private readonly string smtpUsername;
        private readonly string smtpPassword;


        public EmailSender(IConfiguration configration)
        {
            smtpServer = configration.GetValue<string>("SmtpSettings:SmtpServer", "");
            smtpPort = configration.GetValue<int>("SmtpSettings:SmtpPort", 0);
            smtpUsername = configration.GetValue<string>("SmtpSettings:SmtpUsername", "");
            smtpPassword = configration.GetValue<string>("SmtpSettings:SmtpPassword", "");
        }

        public bool SendEmail(string senderName, string senderEmail, string toName, string toEmail, string subject, string textContent) 
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                message.Body = new TextPart("html")
                {
                    Text = textContent
                };

                using (var client = new SmtpClient())
                {
                    client.Connect(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);

                    //Note: only needed if the SMTP server requires authentication
                    client.Authenticate(smtpUsername, smtpPassword);
                    client.Send(message);
                    client.Disconnect(true);

                }
                return true;
                
            }
            catch(Exception ex)
            {
                Console.WriteLine("Email Sender Failure \n" + ex.ToString());
                return false;
            }
            
        }
    }
}
