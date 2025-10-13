using FeaneMVC.Application.DTOs.Reservations;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Enums;

namespace FeaneMVC.Application.Mapping;

public static class ReservationMappingExtensions
{
    public static Reservation ToEntity(this CreateReservationDto dto)
    {
        return new Reservation
        {
            ReservationId = Guid.NewGuid(),
            CustomerName = dto.CustomerName,
            PhoneNumber = dto.PhoneNumber,
            UserEmail = dto.UserEmail,
            NumberOfPeople = dto.NumberOfPeople,
            ReservationDate = dto.ReservationDate,
            SpecialRequests = dto.SpecialRequests,
            Occasion = dto.Occasion,
            SeatingPreference = dto.SeatingPreference,
            Amount = dto.Amount,
            Status = ReservationStatus.Pending,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static ReservationDto ToDto(this Reservation reservation)
    {
        return new ReservationDto
        {
            Id = reservation.ReservationId,
            UserId = reservation.UserId,
            CustomerName = reservation.CustomerName,
            ReservationDate = reservation.ReservationDate,
            NumberOfPeople = reservation.NumberOfPeople,
            PhoneNumber = reservation.PhoneNumber,
            UserEmail = reservation.UserEmail ?? string.Empty,
            Occasion = reservation.Occasion,
            SeatingPreference = reservation.SeatingPreference,
            SpecialRequests = reservation.SpecialRequests,
            Status = reservation.Status,
            Amount = reservation.Amount,
            UpdatedAt = reservation.UpdatedAt
        };
    }

    public static IEnumerable<ReservationDto> ToDtoCollection(this IEnumerable<Reservation> reservations)
    {
        return reservations.Select(ToDto);
    }
}
