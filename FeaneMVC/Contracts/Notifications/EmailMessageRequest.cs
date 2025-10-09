using System.ComponentModel.DataAnnotations;

namespace FeaneMVC.Contracts.Notifications;

public class EmailMessageRequest
{
    [Required]
    [EmailAddress]
    [Display(Name = "Recipient email")]
    public string RecipientEmail { get; set; } = string.Empty;

    [StringLength(128)]
    [Display(Name = "Subject")]
    public string? Subject { get; set; }

    [Required]
    [StringLength(2000, MinimumLength = 5)]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;
}
