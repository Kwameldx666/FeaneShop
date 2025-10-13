using FluentValidation;

namespace FeaneMVC.Application.Queries.Carts.Validators;

public class GetCartQueryValidator : AbstractValidator<GetCartQuery>
{
    public GetCartQueryValidator()
    {
        RuleFor(query => query.UserId)
            .NotEmpty();
    }
}
