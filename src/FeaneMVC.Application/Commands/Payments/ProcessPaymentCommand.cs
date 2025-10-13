using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Payments;

public record ProcessPaymentCommand(Guid UserId, PaymentDetails PaymentDetails) : IRequest<OperationResult<PaymentReceipt>>;
