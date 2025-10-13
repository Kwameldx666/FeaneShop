using FeaneMVC.Domain.Entities;
using FluentValidation;

namespace FeaneMVC.Application.Validation;

public class PaymentDetailsValidator : AbstractValidator<PaymentDetails>
{
    public PaymentDetailsValidator()
    {
        RuleFor(payment => payment.CardNumber)
            .NotEmpty()
            .CreditCard();

        RuleFor(payment => payment.CardHolderName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(payment => payment.ExpiryDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("The card has expired.");

        RuleFor(payment => payment.CVV)
            .NotEmpty()
            .Matches("^\\d{3,4}$")
            .WithMessage("CVV must be 3 or 4 digits.");

        RuleFor(payment => payment.Amount)
            .GreaterThan(0);

        RuleFor(payment => payment.TotalPrice)
            .GreaterThan(0);

        RuleFor(payment => payment.Currency)
            .NotEmpty();
    }
}
