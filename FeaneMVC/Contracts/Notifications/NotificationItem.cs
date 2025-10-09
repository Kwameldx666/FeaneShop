using System;

namespace FeaneMVC.Contracts.Notifications;

public class NotificationItem
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
