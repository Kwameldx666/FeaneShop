using FluentValidation;

namespace FeaneMVC.Application.Commands.Payments.Validators;

public class ProcessRefundCommandValidator : AbstractValidator<ProcessRefundCommand>
{
    public ProcessRefundCommandValidator()
    {
        RuleFor(command => command.TransactionId)
            .NotEmpty();

        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
