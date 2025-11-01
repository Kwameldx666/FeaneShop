using System.ComponentModel.DataAnnotations;

namespace ReservationService.Application.DTOs;

public class ReservationCreateRequest
{
    [Required] public Guid UserId { get; set; }

    [Required] [StringLength(100)] public string CustomerName { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^\\+?[0-9\\s\\-\\(\\)]+$",
        ErrorMessage =
            "PhoneNumber must contain only digits, spaces, parentheses, dashes, and an optional leading plus sign.")]
    [StringLength(32)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(128)]
    public string UserEmail { get; set; } = string.Empty;

    [Range(1, 50)] public int NumberOfPeople { get; set; } = 2;

    [Required] public DateTime ReservationDateTime { get; set; } = DateTime.UtcNow.AddHours(2);

    [StringLength(64)] public string? Occasion { get; set; }

    [StringLength(64)] public string? SeatingPreference { get; set; }

    [StringLength(1024)] public string? SpecialRequests { get; set; }

    [Range(0, 10000)] public decimal? BudgetPerGuest { get; set; }
}