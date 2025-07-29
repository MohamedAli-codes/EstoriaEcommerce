using SendGrid.Helpers.Mail;
using SendGrid;
using Resend;
using System.Net.Mail;
using System.Net;

namespace E_commerce.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private IConfiguration config;

        public SmtpEmailSender(IConfiguration config)
        {
            this.config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string messageHtml)
        {
            var smtpClient = new SmtpClient(config["Smtp:Host"], int.Parse(config["Smtp:Port"])) 
            { 
                Credentials= new NetworkCredential(config["Smtp:Username"],config["Smtp:Password"]) ,
                EnableSsl = true 
            };

            var mailMessage = new MailMessage() 
            { From = new MailAddress(config["Smtp:From"]),
                Subject = subject,
                Body = messageHtml,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message.ToString());
            }
        }
    }
}
