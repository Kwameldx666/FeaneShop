namespace ReservationService.Application.DTOs;

public record ReservationResponse(
    Guid Id,
    Guid? UserId,
    string CustomerName,
    string PhoneNumber,
    string UserEmail,
    int NumberOfPeople,
    DateTime ReservationDate,
    string Status,
    string? Occasion,
    string? SeatingPreference,
    string? SpecialRequests,
    decimal BudgetPerGuest,
    decimal EstimatedTotal,
    bool CanCancel,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? CancelledAt);