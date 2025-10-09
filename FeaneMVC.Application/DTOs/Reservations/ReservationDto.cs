using System;
using FeaneMVC.Domain.Enums;

namespace FeaneMVC.Application.DTOs.Reservations;

public class ReservationDto
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public DateTime ReservationDate { get; init; }

    public int NumberOfPeople { get; init; }

    public string PhoneNumber { get; init; } = string.Empty;

    public string UserEmail { get; init; } = string.Empty;

    public string? Occasion { get; init; }

    public string? SeatingPreference { get; init; }

    public string? SpecialRequests { get; init; }

    public ReservationStatus Status { get; init; }

    public decimal Amount { get; init; }

    public DateTime UpdatedAt { get; init; }
}
