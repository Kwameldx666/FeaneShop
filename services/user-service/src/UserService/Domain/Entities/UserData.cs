using System.ComponentModel.DataAnnotations;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities;

/// <summary>
/// User profile stored in the user-service. Authentication data is owned by the auth-service.
/// </summary>
public class UserData
{
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the user in the authentication service. Mirrors <see cref="Id"/>.
    /// </summary>
    [Required]
    public Guid AuthUserId { get; set; }


    [Required]
    public string Username { get; set; } = string.Empty;

    public string? NormalizedUserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? NormalizedEmail { get; set; }

    [Required]
    public Role Roles { get; set; }

    public bool IsActive { get; set; }

    public DateTime FirstRegisterTime { get; set; }

    public DateTime? FirstLoginTime { get; set; }

    public string? IP { get; set; }

    public string? Address { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public Guid DeliveryId { get; set; }
    public DeliveryAddress? Delivery { get; set; }
}
