namespace UserService.Application.Interfaces;

public interface INotificationService
{
    void SendNotification(string message, string recipientEmail, string? subject = null);
}