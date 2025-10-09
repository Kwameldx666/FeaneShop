using FluentValidation;

namespace FeaneMVC.Application.Queries.Carts.Validators;

public class CalculateCartTotalQueryValidator : AbstractValidator<CalculateCartTotalQuery>
{
    public CalculateCartTotalQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty();
    }
}
