using FluentValidation;

namespace FeaneMVC.Application.Commands.Carts.Validators;

public class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.DishId)
            .NotEmpty();
    }
}
