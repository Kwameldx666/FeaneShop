namespace FeaneMVC.Application.DTOs.Analytics;

/// <summary>
/// Aggregated metrics for the analytics dashboard.
/// </summary>
public class AnalyticsSummary
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalReservations { get; set; }
    public int ReservationsToday { get; set; }
    public int ReservationsThisWeek { get; set; }
    public int UpcomingReservations { get; set; }
    public int CancelledReservations { get; set; }
    public int CompletedReservations { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisWeek { get; set; }
    public decimal AverageReservationValue { get; set; }

    public IReadOnlyList<RevenueTrendPoint> RevenueTrend { get; set; } = new List<RevenueTrendPoint>();
    public IReadOnlyList<ReservationStatusBreakdown> ReservationStatuses { get; set; } = new List<ReservationStatusBreakdown>();
}
