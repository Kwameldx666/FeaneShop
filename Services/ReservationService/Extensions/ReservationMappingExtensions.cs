using Feane.Contracts.Reservations;
using ReservationService.Models;
using System.Linq;

namespace ReservationService.Extensions;

internal static class ReservationMappingExtensions
{
    public static ReservationHistoryItem ToHistoryItem(this ReservationDocument document)
    {
        return new ReservationHistoryItem
        {
            Id = document.Id,
            ReservationDate = document.ReservationDateTime,
            CustomerName = document.CustomerName,
            NumberOfPeople = document.NumberOfPeople,
            Status = document.Status,
            Amount = document.BudgetPerGuest * document.NumberOfPeople,
            Occasion = document.Occasion,
            SeatingPreference = document.SeatingPreference,
            SpecialRequests = document.SpecialRequests
        };
    }

    public static IEnumerable<ReservationHistoryItem> ToHistoryItems(this IEnumerable<ReservationDocument> reservations)
    {
        return reservations.Select(ToHistoryItem);
    }
}
