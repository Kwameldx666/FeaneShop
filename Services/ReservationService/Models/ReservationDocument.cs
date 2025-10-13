using Feane.Contracts.Reservations;

namespace ReservationService.Models;

public sealed class ReservationDocument
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReservationDateTime { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int NumberOfPeople { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public decimal BudgetPerGuest { get; set; }
    public string? Occasion { get; set; }
    public string? SeatingPreference { get; set; }
    public string? SpecialRequests { get; set; }
    public DateTime CreatedAt { get; set; }
}
