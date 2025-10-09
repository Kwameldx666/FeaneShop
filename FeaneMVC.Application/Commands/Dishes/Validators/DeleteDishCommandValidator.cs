using FluentValidation;

namespace FeaneMVC.Application.Commands.Dishes.Validators;

public class DeleteDishCommandValidator : AbstractValidator<DeleteDishCommand>
{
    public DeleteDishCommandValidator()
    {
        RuleFor(command => command.DishId)
            .NotEmpty();
    }
}
