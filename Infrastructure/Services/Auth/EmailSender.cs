using Application.Interfaces.Services.Auth;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;

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
            var apiKey = _configuration["SendGridSettings:ApiKey"];
            var fromEmail = _configuration["SendGridSettings:FromEmail"];
            var fromName = _configuration["SendGridSettings:FromName"];

            if (string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("SendGrid settings are not configured correctly.");
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
