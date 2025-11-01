using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IDishRepository _dishRepository;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IDishRepository dishRepository, ILogger<AnalyticsController> logger)
    {
        _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analytics summary requested");

        var dishes = await _dishRepository.GetAsync(new DishQueryOptions
        {
            SortBy = DishSortField.CreatedAt,
            Descending = true
        }, cancellationToken);

        if (dishes.Count == 0)
            return Ok(new
            {
                metrics = Array.Empty<object>()
            });

        var available = dishes.Where(d => d.IsAvailable).ToList();
        var featured = available.Where(d => d.IsFeatured).ToList();
        var averagePrice = available.Count > 0 ? Math.Round(available.Average(d => d.Price), 2) : 0m;
        var topCategory = available
            .GroupBy(d => d.Category)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .FirstOrDefault()?.Key ?? "—";
        var lastUpdated = dishes.Max(d => d.UpdatedAt);
        var totalMenuValue = available.Sum(d => d.Price);

        var metrics = new object[]
        {
            new { title = "Total dishes", value = dishes.Count },
            new { title = "Available today", value = available.Count },
            new { title = "Featured picks", value = featured.Count },
            new { title = "Average price", value = averagePrice },
            new { title = "Menu value", value = Math.Round(totalMenuValue, 2) },
            new { title = "Top category", value = topCategory },
            new { title = "Last update", value = lastUpdated.ToUniversalTime().ToString("u") }
        };

        return Ok(new { metrics });
    }
}