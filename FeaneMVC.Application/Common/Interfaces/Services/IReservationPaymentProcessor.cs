using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;

namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface IReservationPaymentProcessor
{
    OperationResult<PaymentReceipt> ProcessPayment(Reservation reservation, PaymentDetails paymentDetails);

    OperationResult<PaymentReceipt> RefundPayment(Guid reservationId);
}
