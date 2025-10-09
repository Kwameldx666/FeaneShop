using FluentValidation;

namespace FeaneMVC.Application.Commands.Payments.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator(IValidator<Domain.Entities.PaymentDetails> paymentValidator)
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.PaymentDetails)
            .NotNull()
            .SetValidator(paymentValidator);
    }
}
