using System.ComponentModel.DataAnnotations;

namespace FeaneMVC.Application.DTOs.Reservations;

public class CreateReservationDto
{
    [Required]
    public string CustomerName { get; init; } = string.Empty;

    [Required]
    public string PhoneNumber { get; init; } = string.Empty;

    [EmailAddress]
    public string UserEmail { get; init; } = string.Empty;

    [Range(1, 20)]
    public int NumberOfPeople { get; init; }

    [Required]
    public DateTime ReservationDate { get; init; }

    public string? Occasion { get; init; }

    public string? SeatingPreference { get; init; }

    public string? SpecialRequests { get; init; }

    [Range(0, double.MaxValue)]
    public decimal Amount { get; init; }
}
