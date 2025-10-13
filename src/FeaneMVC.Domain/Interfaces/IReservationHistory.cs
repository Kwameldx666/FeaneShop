using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;

namespace FeaneMVC.Domain.Interfaces;

public interface IReservationHistory
{
    IEnumerable<ReservationHistory> GetReservationHistoryByUserId(Guid userId);

    IEnumerable<ReservationHistory> GetReservationHistoryByItemId(Guid itemId);

    OperationResult<ReservationHistory> AddReservationHistory(ReservationHistory history);
}
