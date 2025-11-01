using System.Net;
using System.Net.Mail;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void SendNotification(string message, string recipientEmail, string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            _logger.LogWarning("Cannot send notification: recipient email is missing.");
            return;
        }

        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var portValue = _configuration["EmailSettings:Port"];
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderName = _configuration["EmailSettings:SenderName"] ?? senderEmail;
        var password = _configuration["EmailSettings:Password"];

        if (!int.TryParse(portValue, out var port))
        {
            _logger.LogError("EmailSettings:Port is not configured correctly.");
            return;
        }

        if (string.IsNullOrWhiteSpace(smtpServer) || string.IsNullOrWhiteSpace(senderEmail) ||
            string.IsNullOrWhiteSpace(password))
        {
            _logger.LogError("Email notification settings are incomplete.");
            return;
        }

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(senderEmail, senderName),
            Subject = string.IsNullOrWhiteSpace(subject) ? "Notification" : subject,
            Body = message,
            IsBodyHtml = true
        };

        mailMessage.To.Add(recipientEmail);

        using var smtpClient = new SmtpClient(smtpServer, port)
        {
            Credentials = new NetworkCredential(senderEmail, password),
            EnableSsl = port == 587
        };

        try
        {
            smtpClient.Send(mailMessage);
            _logger.LogInformation("Notification sent to {Recipient}", recipientEmail);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to send notification to {Recipient}", recipientEmail);
        }
    }
}