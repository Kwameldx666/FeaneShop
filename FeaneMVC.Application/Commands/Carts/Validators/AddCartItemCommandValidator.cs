using FluentValidation;

namespace FeaneMVC.Application.Commands.Carts.Validators;

public class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator(IValidator<Domain.Entities.CartItem> cartItemValidator)
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.Item)
            .NotNull()
            .SetValidator(cartItemValidator);
    }
}
