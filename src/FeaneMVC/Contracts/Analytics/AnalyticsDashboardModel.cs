using FeaneMVC.Application.DTOs.Analytics;

namespace FeaneMVC.Contracts.Analytics;

public class AnalyticsDashboardModel
{
    public AnalyticsSummary Summary { get; set; } = new();

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool HasCustomRange => StartDate.HasValue || EndDate.HasValue;
}
