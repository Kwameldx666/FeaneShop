using FluentValidation;

namespace FeaneMVC.Application.Queries.Sessions.Validators;

public class GetUserByCookieQueryValidator : AbstractValidator<GetUserByCookieQuery>
{
    public GetUserByCookieQueryValidator()
    {
        RuleFor(query => query.CookieValue)
            .NotEmpty();
    }
}
