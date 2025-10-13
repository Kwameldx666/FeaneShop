using FeaneMVC.Domain.Entities;

namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface IReservationNotifier
{
    void SendReservationConfirmation(Reservation reservation);

    void SendReservationCancellation(Reservation reservation);
}
