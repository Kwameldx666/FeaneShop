namespace FeaneMVC.Contracts.Notifications;

public class NotificationsPageModel
{
    public Dictionary<string, IReadOnlyList<NotificationItem>> Groups { get; set; } = new();

    public EmailMessageRequest Email { get; set; } = new();

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }
}
