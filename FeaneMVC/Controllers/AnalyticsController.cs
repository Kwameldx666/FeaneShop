using FeaneMVC.Attributes;
using FeaneMVC.Application.Queries.Analytics;
using FeaneMVC.Contracts.Analytics;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace FeaneMVC.Controllers
{
    [ServiceFilter(typeof(AdminOrModeratorModeAttribute))]
    public class AnalyticsController : Controller
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                ModelState.AddModelError(string.Empty, "Start date must be earlier than the end date.");
                (startDate, endDate) = (endDate, startDate);
            }

            var summary = await _mediator.Send(new GetAnalyticsSummaryQuery(startDate, endDate));

            var viewModel = new AnalyticsDashboardModel
            {
                Summary = summary,
                StartDate = startDate,
                EndDate = endDate
            };

            return View(viewModel);
        }
    }
}
