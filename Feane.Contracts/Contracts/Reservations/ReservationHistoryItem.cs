namespace Feane.Contracts.Reservations;

public class ReservationHistoryItem
{
    public Guid ReservationId { get; set; }

    public DateTime ReservationDate { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int NumberOfPeople { get; set; }

    public string Status { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string? Occasion { get; set; }

    public string? SeatingPreference { get; set; }

    public string? SpecialRequests { get; set; }

    public DateTime UpdatedAt { get; set; }
}
