using FeaneMVC.Application.Commands.Reservations;
using FeaneMVC.Application.DTOs.Reservations;
using FeaneMVC.Contracts.Reservations;

namespace FeaneMVC.Extenstions;

public static class ReservationContractMappingExtensions
{
    public static CreateReservationCommand ToCommand(this CreateReservationRequest request, Guid userId)
    {
        var createDto = new CreateReservationDto
        {
            CustomerName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            UserEmail = request.UserEmail,
            NumberOfPeople = request.NumberOfPeople,
            ReservationDate = request.ReservationDateTime,
            Occasion = request.Occasion,
            SeatingPreference = request.SeatingPreference,
            SpecialRequests = request.SpecialRequests,
            Amount = Math.Round(request.NumberOfPeople * request.BudgetPerGuest, 2)
        };

        return new CreateReservationCommand(createDto, userId);
    }

    public static ReservationHistoryItem ToHistoryItem(this ReservationDto reservation)
    {
        return new ReservationHistoryItem
        {
            Id = reservation.Id,
            ReservationDate = reservation.ReservationDate,
            CustomerName = reservation.CustomerName,
            NumberOfPeople = reservation.NumberOfPeople,
            Status = reservation.Status,
            Amount = reservation.Amount,
            Occasion = reservation.Occasion,
            SeatingPreference = reservation.SeatingPreference,
            SpecialRequests = reservation.SpecialRequests
        };
    }

    public static IReadOnlyList<ReservationHistoryItem> ToHistoryItems(this IEnumerable<ReservationDto> reservations)
    {
        return reservations.Select(ToHistoryItem).OrderByDescending(reservation => reservation.ReservationDate).ToList();
    }
}
