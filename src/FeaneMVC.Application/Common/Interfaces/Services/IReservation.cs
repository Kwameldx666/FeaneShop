using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;

namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface IReservation
{
    OperationResult<Reservation> CreateReservation(Reservation reservation, Guid userId);

    OperationResult<Reservation> CancelReservation(Guid reservationId);

    OperationResult<Reservation> GetReservationById(Guid reservationId);

    IEnumerable<Reservation> GetAllReservations();

    IEnumerable<Reservation> GetReservationsByUserId(Guid userId);

    OperationResult<Reservation> UpdateReservation(Guid reservationId, Reservation reservation);
}
