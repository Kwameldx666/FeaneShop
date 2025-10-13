using FluentValidation;

namespace FeaneMVC.Application.Queries.Analytics.Validators;

public class GetAnalyticsSummaryQueryValidator : AbstractValidator<GetAnalyticsSummaryQuery>
{
    public GetAnalyticsSummaryQueryValidator()
    {
        When(query => query.StartDate.HasValue && query.EndDate.HasValue, () =>
        {
            RuleFor(query => query.StartDate)
                .LessThanOrEqualTo(query => query.EndDate);
        });
    }
}
