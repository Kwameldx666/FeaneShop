namespace ReservationService.Api;

public class ReservationStore
{
    private readonly List<Reservation> _reservations = new();

    public IReadOnlyCollection<Reservation> GetReservations() => _reservations;

    public Reservation Create(CreateReservationRequest request)
    {
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            Guests = request.Guests,
            ReservationTime = request.ReservationTime,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _reservations.Add(reservation);
        return reservation;
    }

    public Reservation? Cancel(Guid id)
    {
        var reservation = _reservations.FirstOrDefault(r => r.Id == id);
        if (reservation is null)
        {
            return null;
        }

        reservation.Status = ReservationStatus.Cancelled;
        return reservation;
    }
}

public class Reservation
{
    public Guid Id { get; init; }
    public required string CustomerName { get; init; }
    public required string CustomerPhone { get; init; }
    public required int Guests { get; init; }
    public required DateTimeOffset ReservationTime { get; init; }
    public ReservationStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record CreateReservationRequest(string CustomerName, string CustomerPhone, int Guests, DateTimeOffset ReservationTime);

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled
}
