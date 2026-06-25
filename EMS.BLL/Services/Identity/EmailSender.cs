using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace EMS.BLL.Services.Identity
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpHost = _config["MailSettings:Host"];
            var smtpPort = int.TryParse(_config["MailSettings:Port"], out var p) ? p : 25;
            var smtpUser = _config["MailSettings:Email"];
            var smtpPass = _config["MailSettings:Password"];
            var fromEmail = _config["MailSettings:Email"];

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Recipient email is required.", nameof(email));

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(smtpUser) ||
                string.IsNullOrWhiteSpace(smtpPass) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                _logger.LogWarning("EmailSender: SMTP settings are missing. Configure MailSettings using User Secrets, environment variables, or a secure secret store.");
                throw new InvalidOperationException("SMTP settings are not configured.");
            }

            try
            {
                _logger.LogDebug("EmailSender: sending email to {To} via {Host}:{Port} from {From}", email, smtpHost, smtpPort, fromEmail);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                using var mailMessage = new MailMessage(fromEmail, email, subject, htmlMessage)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("EmailSender: email sent to {To}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmailSender: failed to send email to {To} via {Host}:{Port}", email, smtpHost, smtpPort);
                throw;
            }
        }
    }
}
