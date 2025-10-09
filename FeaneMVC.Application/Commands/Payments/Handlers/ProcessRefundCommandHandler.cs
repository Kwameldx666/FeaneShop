using FeaneMVC.Application.Common.Interfaces.Persistence;
using MediatR;
using INotification = FeaneMVC.Application.Common.Interfaces.Services.INotification;

namespace FeaneMVC.Application.Commands.Payments.Handlers;

public class ProcessRefundCommandHandler : IRequestHandler<ProcessRefundCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotification _notification;

    public ProcessRefundCommandHandler(IUnitOfWork unitOfWork, INotification notification)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _notification = notification ?? throw new ArgumentNullException(nameof(notification));
    }

    public async Task<bool> Handle(ProcessRefundCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TransactionId))
        {
            return false;
        }

        var paymentRepository = _unitOfWork.Payments;

        var paymentRecord = await paymentRepository.FindPaymentByTransactionIdAsync(request.TransactionId, cancellationToken);
        if (paymentRecord == null)
        {
            return false;
        }

        paymentRecord.IsRefunded = true;
        paymentRecord.DateRefunded = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.UserId != Guid.Empty)
        {
            var user = await paymentRepository.FindUserAsync(request.UserId, cancellationToken);
            if (user != null)
            {
                var message = $"Dear {user.Username}, your refund for transaction ID {request.TransactionId} has been processed.";
                _notification.SendNotification(message, user.Email);
            }
        }

        return true;
    }
}
