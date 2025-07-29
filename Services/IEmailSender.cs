namespace E_commerce.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string messageHtml);
    }
}
