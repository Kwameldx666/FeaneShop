using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;

namespace FeaneMVC.Application.Common.Interfaces.Services;

public interface IPaymentGateway
{
    OperationResult<PaymentReceipt> ProcessPayment(Guid userId, PaymentDetails paymentDetails);
}
