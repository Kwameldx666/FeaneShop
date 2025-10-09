using FluentValidation;

namespace FeaneMVC.Application.Commands.Dishes.Validators;

public class UpdateDishCommandValidator : AbstractValidator<UpdateDishCommand>
{
    public UpdateDishCommandValidator()
    {
        RuleFor(command => command.DishId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(command => command.Price)
            .GreaterThan(0);

        RuleFor(command => command.Category)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(command => command.ImageUrl)
            .MaximumLength(300)
            .When(command => !string.IsNullOrWhiteSpace(command.ImageUrl));
    }
}
