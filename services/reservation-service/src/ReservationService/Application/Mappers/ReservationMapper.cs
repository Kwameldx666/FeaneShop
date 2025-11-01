using ReservationService.Application.DTOs;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Enums;

namespace ReservationService.Application.Mappers;

public static class ReservationMapper
{
    public static ReservationResponse ToResponse(this Reservation reservation)
    {
        if (reservation == null) throw new ArgumentNullException(nameof(reservation));

        return new ReservationResponse(
            reservation.Id,
            reservation.UserId,
            reservation.CustomerName,
            reservation.PhoneNumber,
            reservation.UserEmail,
            reservation.NumberOfPeople,
            reservation.ReservationDate,
            reservation.Status.ToString(),
            reservation.Occasion,
            reservation.SeatingPreference,
            reservation.SpecialRequests,
            reservation.BudgetPerGuest,
            reservation.EstimatedTotal,
            CanCancel(reservation),
            reservation.CreatedAt,
            reservation.UpdatedAt,
            reservation.CancelledAt);
    }

    private static bool CanCancel(Reservation reservation)
    {
        if (reservation.Status is ReservationStatus.Cancelled or ReservationStatus.Completed) return false;

        if (reservation.CancelledAt.HasValue) return false;

        return reservation.ReservationDate > DateTime.UtcNow.AddMinutes(-30);
    }
}