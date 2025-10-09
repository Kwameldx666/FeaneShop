using FluentValidation;

namespace FeaneMVC.Application.Queries.Dishes.Validators;

public class GetDishByIdQueryValidator : AbstractValidator<GetDishByIdQuery>
{
    public GetDishByIdQueryValidator()
    {
        RuleFor(query => query.DishId)
            .NotEmpty();
    }
}
