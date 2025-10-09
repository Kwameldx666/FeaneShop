using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Application.DTOs.Analytics;
using FeaneMVC.Domain.Enums;
using MediatR;

namespace FeaneMVC.Application.Queries.Analytics.Handlers;

public class GetAnalyticsSummaryQueryHandler : IRequestHandler<GetAnalyticsSummaryQuery, AnalyticsSummary>
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetAnalyticsSummaryQueryHandler(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository ?? throw new ArgumentNullException(nameof(analyticsRepository));
    }

    public async Task<AnalyticsSummary> Handle(GetAnalyticsSummaryQuery request, CancellationToken cancellationToken)
    {
        var (rangeStart, rangeEnd) = NormalizeRange(request.StartDate, request.EndDate);

        var totalUsers = await _analyticsRepository.GetTotalUsersAsync(cancellationToken);
        var activeUsers = await _analyticsRepository.GetActiveUsersAsync(cancellationToken);
        var reservations = await _analyticsRepository.GetReservationsAsync(rangeStart, rangeEnd, cancellationToken);
        var payments = await _analyticsRepository.GetPaymentsAsync(rangeStart, rangeEnd, cancellationToken);
        var refunds = await _analyticsRepository.GetRefundedPaymentsAsync(rangeStart, rangeEnd, cancellationToken);

        var now = DateTime.UtcNow;
        var today = now.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);
        var sevenDaysAgo = today.AddDays(-6);

        var completedStatuses = new[] { ReservationStatus.Paid, ReservationStatus.Confirmed };
        var upcomingStatuses = new[] { ReservationStatus.Pending, ReservationStatus.Confirmed };

        var summary = new AnalyticsSummary
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalReservations = reservations.Count,
            ReservationsToday = reservations.Count(reservation => reservation.ReservationDate.Date == today),
            ReservationsThisWeek = reservations.Count(reservation => reservation.ReservationDate >= startOfWeek && reservation.ReservationDate < endOfWeek),
            UpcomingReservations = reservations.Count(reservation => reservation.ReservationDate >= now && upcomingStatuses.Contains(reservation.Status)),
            CancelledReservations = reservations.Count(reservation => reservation.Status == ReservationStatus.Canceled),
            CompletedReservations = reservations.Count(reservation => completedStatuses.Contains(reservation.Status)),
            TotalRevenue = payments.Sum(payment => payment.Amount) - refunds.Sum(refund => refund.Amount),
            RevenueThisWeek = payments
                .Where(payment => payment.DateProcessed.Date >= sevenDaysAgo && payment.DateProcessed.Date <= today)
                .Sum(payment => payment.Amount)
                - refunds
                    .Where(refund => refund.DateRefunded!.Value.Date >= sevenDaysAgo && refund.DateRefunded!.Value.Date <= today)
                    .Sum(refund => refund.Amount),
            AverageReservationValue = reservations.Count > 0
                ? reservations.Where(reservation => reservation.Amount > 0).DefaultIfEmpty().Average(reservation => reservation?.Amount ?? 0)
                : 0,
        };

        summary.ReservationStatuses = Enum.GetValues(typeof(ReservationStatus))
            .Cast<ReservationStatus>()
            .Select(status => new ReservationStatusBreakdown
            {
                Status = status,
                Count = reservations.Count(reservation => reservation.Status == status)
            })
            .ToList();

        var trendEnd = (request.EndDate ?? today).Date;
        var trendStart = (request.StartDate ?? trendEnd.AddDays(-6)).Date;
        if (trendEnd < trendStart)
        {
            (trendStart, trendEnd) = (trendEnd, trendStart);
        }

        summary.RevenueTrend = await BuildRevenueTrendAsync(trendStart, trendEnd, cancellationToken);

        return summary;
    }

    private async Task<IReadOnlyList<RevenueTrendPoint>> BuildRevenueTrendAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var exclusiveEndDate = endDate.AddDays(1);
        var payments = await _analyticsRepository.GetPaymentsAsync(startDate, exclusiveEndDate, cancellationToken);
        var refunds = await _analyticsRepository.GetRefundedPaymentsAsync(startDate, exclusiveEndDate, cancellationToken);

        var trendByDate = payments
            .GroupBy(payment => payment.DateProcessed.Date)
            .ToDictionary(
                group => group.Key,
                group => new RevenueTrendPoint
                {
                    Date = group.Key,
                    TotalAmount = group.Sum(payment => payment.Amount),
                    TransactionCount = group.Count()
                });

        foreach (var refundGroup in refunds.GroupBy(refund => refund.DateRefunded!.Value.Date))
        {
            if (!trendByDate.TryGetValue(refundGroup.Key, out var existingPoint))
            {
                existingPoint = new RevenueTrendPoint
                {
                    Date = refundGroup.Key,
                    TotalAmount = 0,
                    TransactionCount = 0
                };

                trendByDate[refundGroup.Key] = existingPoint;
            }

            existingPoint.TotalAmount -= refundGroup.Sum(refund => refund.Amount);
            existingPoint.TransactionCount += refundGroup.Count();
        }

        var trendPoints = new List<RevenueTrendPoint>();
        for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
        {
            if (trendByDate.TryGetValue(day, out var point))
            {
                trendPoints.Add(point);
            }
            else
            {
                trendPoints.Add(new RevenueTrendPoint
                {
                    Date = day,
                    TotalAmount = 0,
                    TransactionCount = 0
                });
            }
        }

        return trendPoints;
    }

    private static (DateTime?, DateTime?) NormalizeRange(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        return (startDate, endDate);
    }
}
