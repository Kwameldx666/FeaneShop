using ReservationService.Domain.Enums;

namespace ReservationService.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; }
    public DateTime ReservationDate { get; set; }
    public string? Occasion { get; set; }
    public string? SeatingPreference { get; set; }
    public string? SpecialRequests { get; set; }
    public decimal BudgetPerGuest { get; set; }
    public decimal EstimatedTotal { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}