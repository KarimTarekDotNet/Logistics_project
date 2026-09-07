using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Extensions.HealthChecks
{
    public class SmtpHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpHealthCheck> _logger;

        public SmtpHealthCheck(IConfiguration configuration, ILogger<SmtpHealthCheck> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var fromPass = _configuration.GetValue<string>("EmailSettings:Password");
            var fromEmail = _configuration.GetValue<string>("EmailSettings:FromEmail");

            if (string.IsNullOrWhiteSpace(fromPass) || string.IsNullOrWhiteSpace(fromEmail))
                return HealthCheckResult.Unhealthy("Email settings are missing.");

            using SmtpClient smtpClient = new SmtpClient();
            try
            {
                await smtpClient.ConnectAsync(_configuration.GetValue<string>("EmailSettings:Host")!,
                _configuration.GetValue<int>("EmailSettings:Port"), SecureSocketOptions.StartTls, cancellationToken);

                await smtpClient.AuthenticateAsync(fromEmail, fromPass, cancellationToken);

                return HealthCheckResult.Healthy("Email service is responding.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP health check failed.");
                return HealthCheckResult.Unhealthy(ex.Message);
            }
            finally
            {
                // Cleanly log out and disconnect from the server
                if (smtpClient.IsConnected)
                    await smtpClient.DisconnectAsync(true, cancellationToken);
            }
        }
    }
}