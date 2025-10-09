using FeaneMVC.Domain.Entities;
using FluentValidation;

namespace FeaneMVC.Application.Validation;

public class ReservationValidator : AbstractValidator<Reservation>
{
    public ReservationValidator()
    {
        RuleFor(reservation => reservation.CustomerName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(reservation => reservation.ReservationDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Reservation date must be today or later.");

        RuleFor(reservation => reservation.NumberOfPeople)
            .InclusiveBetween(1, 20);

        RuleFor(reservation => reservation.PhoneNumber)
            .NotEmpty();

        RuleFor(reservation => reservation.Status)
            .IsInEnum();

        RuleFor(reservation => reservation.Amount)
            .GreaterThanOrEqualTo(0);
    }
}
