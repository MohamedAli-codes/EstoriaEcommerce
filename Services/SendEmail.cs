using SendGrid.Helpers.Mail;
using SendGrid;
using Resend;

namespace E_commerce.Services
{
    public class SendEmail
    {
        private readonly IResend _resend;
        public SendEmail(IResend resend)
        {
            _resend = resend;
        }

        public async Task SendEmailAsync( string senderEmail,  string recieverEmail , string subject,string? HtmlContent)
        {
            var message = new EmailMessage() ;
            message.From = senderEmail;
            message.To.Add(recieverEmail);
            message.Subject = subject;
            message.HtmlBody = HtmlContent;

            var result = await _resend.EmailSendAsync(message);
            Console.WriteLine(result.Content);
        }
    }
}
