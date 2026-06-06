using Application.Interfaces.Services.Auth;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Infrastructure.Services.Auth
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var apiKey = _configuration.GetValue<string>("SendGridSettings:ApiKey");
            var fromEmail = _configuration.GetValue<string>("SendGridSettings:FromEmail");
            var fromName = _configuration.GetValue<string>("SendGridSettings:FromName");

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new BusinessRuleException("SendGrid settings are not configured correctly.");
            }

            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(fromEmail, fromName);
            var toEmail = new EmailAddress(to);

            var msg = MailHelper.CreateSingleEmail(
                from,
                toEmail,
                subject,
                plainTextContent: body,
                htmlContent: body
            );

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Body.ReadAsStringAsync();
                throw new Exception($"Failed to send email. Status: {response.StatusCode}, Body: {errorBody}");
            }
        }
    }
}
