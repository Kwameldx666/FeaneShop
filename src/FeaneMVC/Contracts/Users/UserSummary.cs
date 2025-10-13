using FeaneMVC.Domain.Enums;

namespace FeaneMVC.Contracts.Users;

public class UserSummary
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Role Roles { get; set; }

    public bool IsActive { get; set; }
}
