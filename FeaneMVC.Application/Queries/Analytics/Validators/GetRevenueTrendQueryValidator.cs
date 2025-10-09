using FluentValidation;

namespace FeaneMVC.Application.Queries.Analytics.Validators;

public class GetRevenueTrendQueryValidator : AbstractValidator<GetRevenueTrendQuery>
{
    public GetRevenueTrendQueryValidator()
    {
        RuleFor(query => query.StartDate)
            .LessThanOrEqualTo(query => query.EndDate);

        RuleFor(query => query.EndDate)
            .GreaterThan(query => query.StartDate);
    }
}
