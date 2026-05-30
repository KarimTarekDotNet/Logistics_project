namespace Application.Interfaces.Services.Auth
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}