using AnalyticsService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly ILogger<AnalyticsController> _logger;
    private readonly IAnalyticsRepository _repository;

    public AnalyticsController(IAnalyticsRepository repository, ILogger<AnalyticsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    ///     Получить данные для dashboard
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        try
        {
            var data = await _repository.GetDashboardDataAsync(startDate, endDate);
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard data");
            return StatusCode(500, new { success = false, message = "Failed to retrieve dashboard data" });
        }
    }

    /// <summary>
    ///     Получить отчет о выручке за период
    /// </summary>
    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenueReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            if (start > end) return BadRequest(new { success = false, message = "Start date must be before end date" });

            var report = await _repository.GetRevenueReportAsync(start, end);
            return Ok(new { success = true, report });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue report");
            return StatusCode(500, new { success = false, message = "Failed to retrieve revenue report" });
        }
    }

    /// <summary>
    ///     Получить статистику по товарам
    /// </summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProductPerformance([FromQuery] int top = 10)
    {
        try
        {
            if (top < 1 || top > 100)
                return BadRequest(new { success = false, message = "Top parameter must be between 1 and 100" });

            var performance = await _repository.GetProductPerformanceAsync(top);
            return Ok(new { success = true, performance });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product performance");
            return StatusCode(500, new { success = false, message = "Failed to retrieve product performance" });
        }
    }

    /// <summary>
    ///     Записать событие аналитики
    /// </summary>
    [HttpPost("events")]
    public async Task<IActionResult> RecordEvent([FromBody] RecordEventRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.EventType) || string.IsNullOrEmpty(request.EntityType))
                return BadRequest(new { success = false, message = "EventType and EntityType are required" });

            await _repository.RecordEventAsync(
                request.EventType,
                request.EntityType,
                request.EntityId,
                request.Data ?? string.Empty,
                request.UserId
            );

            return Ok(new { success = true, message = "Event recorded" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording analytics event");
            return StatusCode(500, new { success = false, message = "Failed to record event" });
        }
    }
}

public class RecordEventRequest
{
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? Data { get; set; }
    public Guid? UserId { get; set; }
}