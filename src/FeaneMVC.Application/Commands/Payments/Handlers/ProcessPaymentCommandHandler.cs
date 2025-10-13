using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using MediatR;
using INotification = FeaneMVC.Application.Common.Interfaces.Services.INotification;

namespace FeaneMVC.Application.Commands.Payments.Handlers;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, OperationResult<PaymentReceipt>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotification _notification;

    public ProcessPaymentCommandHandler(IUnitOfWork unitOfWork, INotification notification)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _notification = notification ?? throw new ArgumentNullException(nameof(notification));
    }

    public async Task<OperationResult<PaymentReceipt>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var paymentRepository = _unitOfWork.Payments;

            var user = await paymentRepository.FindUserAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return OperationResult<PaymentReceipt>.Failure("User not found.");
            }

            var cart = await paymentRepository.FindCartWithItemsAsync(request.UserId, cancellationToken);
            if (cart != null)
            {
                paymentRepository.RemoveCart(cart);
            }

            var paymentDetails = request.PaymentDetails ?? throw new ArgumentNullException(nameof(request.PaymentDetails));
            var transactionId = Guid.NewGuid().ToString();
            var amount = paymentDetails.Amount > 0 ? paymentDetails.Amount : paymentDetails.TotalPrice;
            var currency = string.IsNullOrWhiteSpace(paymentDetails.Currency) ? "USD" : paymentDetails.Currency;

            var paymentRecord = new PaymentRecord
            {
                Id = Guid.NewGuid(),
                CardNumber = paymentDetails.CardNumber,
                CardHolderName = paymentDetails.CardHolderName,
                ExpiryDate = paymentDetails.ExpiryDate,
                CVV = paymentDetails.CVV,
                Amount = amount,
                Currency = currency,
                TransactionId = transactionId,
                DateProcessed = DateTime.UtcNow
            };

            paymentRepository.AddPaymentRecord(paymentRecord);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var message = $"Dear {user.Username}, your payment of {amount} {currency} has been processed successfully. Transaction ID: {transactionId}.";
            _notification.SendNotification(message, user.Email);

            var receipt = new PaymentReceipt { TransactionId = transactionId };
            return OperationResult<PaymentReceipt>.Success(receipt, "Payment processed successfully.");
        }
        catch (Exception exception)
        {
            return OperationResult<PaymentReceipt>.Failure($"Payment processing failed: {exception.Message}");
        }
    }
}
