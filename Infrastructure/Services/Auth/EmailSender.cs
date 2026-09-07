using Application.Interfaces.Services.Auth;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;

namespace Infrastructure.Services.Auth
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var fromPass = _configuration.GetValue<string>("EmailSettings:Password");
            var fromEmail = _configuration.GetValue<string>("EmailSettings:FromEmail");
            var fromName = _configuration.GetValue<string>("EmailSettings:FromName");

            if (string.IsNullOrWhiteSpace(fromPass) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new BusinessRuleException("Email settings are not configured correctly.");
            }

            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(to));

            message.Subject = subject;
            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = body;

            message.Body = bodyBuilder.ToMessageBody();
            
            using SmtpClient smtpClient = new SmtpClient();

            try
            {
                await smtpClient.ConnectAsync(_configuration.GetValue<string>("EmailSettings:Host")!,
                _configuration.GetValue<int>("EmailSettings:Port"), SecureSocketOptions.StartTls);

                await smtpClient.AuthenticateAsync(fromEmail, fromPass);
                await smtpClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email.");
                throw;
            }
            finally
            {
                // Cleanly log out and disconnect from the server
                if (smtpClient.IsConnected)
                    await smtpClient.DisconnectAsync(true);
            }
        }
    }
}
