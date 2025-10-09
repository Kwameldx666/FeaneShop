using FeaneMVC.Domain.Entities;
using FluentValidation;

namespace FeaneMVC.Application.Validation;

public class CartItemValidator : AbstractValidator<CartItem>
{
    public CartItemValidator()
    {
        RuleFor(item => item.DishId)
            .NotEmpty();

        RuleFor(item => item.UserId)
            .NotEmpty();

        RuleFor(item => item.Quantity)
            .GreaterThan(0);

        RuleFor(item => item.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(item => item.TotalPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(item => item.Name)
            .NotEmpty()
            .MaximumLength(200);
    }
}
