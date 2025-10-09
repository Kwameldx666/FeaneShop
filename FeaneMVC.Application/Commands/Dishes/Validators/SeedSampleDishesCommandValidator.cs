using FluentValidation;

namespace FeaneMVC.Application.Commands.Dishes.Validators;

public class SeedSampleDishesCommandValidator : AbstractValidator<SeedSampleDishesCommand>
{
    public SeedSampleDishesCommandValidator()
    {
        RuleFor(command => command.Count)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}
