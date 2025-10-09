using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Application.DTOs.Analytics;
using FeaneMVC.Application.Queries.Analytics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FeaneMVC.Application.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAnalyticsService"/> that aggregates data directly from the application database.
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(IMediator mediator, ILogger<AnalyticsService> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<AnalyticsSummary> GetSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                return await _mediator.Send(new GetAnalyticsSummaryQuery(startDate, endDate));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to build analytics summary.");
                return new AnalyticsSummary();
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<RevenueTrendPoint>> GetRevenueTrendAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _mediator.Send(new GetRevenueTrendQuery(startDate, endDate));
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to calculate revenue trend.");
                return Array.Empty<RevenueTrendPoint>();
            }
        }
    }
}
