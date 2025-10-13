using FeaneMVC.Application.Commands.Payments;
using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FeaneMVC.Application.Services
{
    public class PaymentProcessor : IPaymentGateway
    {
        private readonly IMediator _mediator;
        private readonly ISessionService _sessionService;
        private readonly ILogger<PaymentProcessor> _logger;

        public PaymentProcessor(IMediator mediator, ISessionService sessionService, ILogger<PaymentProcessor> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public OperationResult<PaymentReceipt> ProcessPayment(Guid userId, PaymentDetails paymentDetails)
        {
            try
            {
                return _mediator.Send(new ProcessPaymentCommand(userId, paymentDetails)).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error processing payment for user {UserId}", userId);
                return OperationResult<PaymentReceipt>.Failure($"Payment processing failed: {exception.Message}");
            }
        }

        public bool ProcessRefund(string transactionId)
        {
            try
            {
                var userId = _sessionService.GetUserId();
                return _mediator.Send(new ProcessRefundCommand(transactionId, userId)).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error processing refund for transaction {TransactionId}", transactionId);
                return false;
            }
        }
    }
}
