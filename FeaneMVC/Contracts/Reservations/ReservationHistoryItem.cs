using System;
using FeaneMVC.Domain.Enums;

namespace FeaneMVC.Contracts.Reservations;

public class ReservationHistoryItem
{
    public Guid Id { get; init; }

    public DateTime ReservationDate { get; init; }

    public string CustomerName { get; init; } = string.Empty;

    public int NumberOfPeople { get; init; }

    public ReservationStatus Status { get; init; }

    public decimal Amount { get; init; }

    public string? Occasion { get; init; }

    public string? SeatingPreference { get; init; }

    public string? SpecialRequests { get; init; }
}
