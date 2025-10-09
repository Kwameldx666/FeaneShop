using FeaneMVC.Domain.Entities;
using FluentValidation;

namespace FeaneMVC.Application.Validation;

public class DishValidator : AbstractValidator<Dish>
{
    public DishValidator()
    {
        RuleFor(dish => dish.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(dish => dish.Description)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(dish => dish.Price)
            .GreaterThan(0);

        RuleFor(dish => dish.Category)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(dish => dish.ImageUrl)
            .MaximumLength(300)
            .When(dish => !string.IsNullOrWhiteSpace(dish.ImageUrl));
    }
}
