using FeaneMVC.Domain.Enums;

namespace FeaneMVC.Application.DTOs.Analytics;

public class ReservationStatusBreakdown
{
    public ReservationStatus Status { get; set; }
    public int Count { get; set; }
}
