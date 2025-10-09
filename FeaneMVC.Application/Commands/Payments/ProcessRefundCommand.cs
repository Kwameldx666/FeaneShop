using MediatR;

namespace FeaneMVC.Application.Commands.Payments;

public record ProcessRefundCommand(string TransactionId, Guid UserId) : IRequest<bool>;
