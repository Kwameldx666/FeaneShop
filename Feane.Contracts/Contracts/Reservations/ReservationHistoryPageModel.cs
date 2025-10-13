namespace Feane.Contracts.Reservations;

public class ReservationHistoryPageModel
{
    public List<ReservationHistoryItem> Items { get; set; } = new();

    public string? StatusMessage { get; set; }

    public string? ErrorMessage { get; set; }
}
