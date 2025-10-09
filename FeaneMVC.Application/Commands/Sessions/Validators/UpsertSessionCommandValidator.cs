using FluentValidation;

namespace FeaneMVC.Application.Commands.Sessions.Validators;

public class UpsertSessionCommandValidator : AbstractValidator<UpsertSessionCommand>
{
    public UpsertSessionCommandValidator()
    {
        RuleFor(command => command.Credential)
            .NotEmpty();

        RuleFor(command => command.CookieValue)
            .NotEmpty();

        RuleFor(command => command.ExpireTime)
            .GreaterThan(DateTimeOffset.UtcNow);
    }
}
