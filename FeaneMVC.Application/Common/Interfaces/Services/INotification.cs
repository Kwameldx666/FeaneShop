namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface INotification
{
    void SendNotification(string message, string recipientEmail, string? subject = null);
}
