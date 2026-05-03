using Calendar.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Calendar.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var section = _configuration.GetSection("SmtpSettings");
            var senderName = section["SenderName"];
            var senderEmail = section["SenderEmail"];
            var password = section["Password"];
            var server = section["Server"];
            var portStr = section["Port"] ?? "587";
            
            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email settings are missing or incomplete in appsettings.json");
                return;
            }

            var port = int.Parse(portStr);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Connect to Gmail SMTP server
            await client.ConnectAsync(server, port, SecureSocketOptions.StartTls);

            // Authenticate with App Password
            await client.AuthenticateAsync(senderEmail, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            
            _logger.LogInformation("Successfully sent email to {To} with subject: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}. Error: {Message}", to, ex.Message);
        }
    }
}
