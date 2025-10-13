namespace Feane.Contracts.Reservations;

public class ReservationHistoryPageModel
{
    public IReadOnlyList<ReservationHistoryItem> Reservations { get; init; } = Array.Empty<ReservationHistoryItem>();

    public string? StatusMessage { get; init; }

    public string? ErrorMessage { get; init; }
}
