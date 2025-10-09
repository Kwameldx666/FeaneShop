using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Application.DTOs.Analytics;
using MediatR;

namespace FeaneMVC.Application.Queries.Analytics.Handlers;

public class GetRevenueTrendQueryHandler : IRequestHandler<GetRevenueTrendQuery, IReadOnlyList<RevenueTrendPoint>>
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetRevenueTrendQueryHandler(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository ?? throw new ArgumentNullException(nameof(analyticsRepository));
    }

    public async Task<IReadOnlyList<RevenueTrendPoint>> Handle(GetRevenueTrendQuery request, CancellationToken cancellationToken)
    {
        var normalizedStart = request.StartDate.Date;
        var normalizedEnd = request.EndDate.Date;

        if (normalizedEnd < normalizedStart)
        {
            (normalizedStart, normalizedEnd) = (normalizedEnd, normalizedStart);
        }

        var exclusiveEnd = normalizedEnd.AddDays(1);

        var payments = await _analyticsRepository.GetPaymentsAsync(normalizedStart, exclusiveEnd, cancellationToken);
        var refunds = await _analyticsRepository.GetRefundedPaymentsAsync(normalizedStart, exclusiveEnd, cancellationToken);

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
        for (var day = normalizedStart; day <= normalizedEnd; day = day.AddDays(1))
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
}
