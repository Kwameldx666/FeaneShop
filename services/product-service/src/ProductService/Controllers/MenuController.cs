using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IDishRepository _dishRepository;
    private readonly ILogger<MenuController> _logger;

    public MenuController(IDishRepository dishRepository, ILogger<MenuController> logger)
    {
        _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetMenu([FromQuery] string? category, [FromQuery] bool? featuredOnly, [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var options = new DishQueryOptions
        {
            Category = category,
            AvailableOnly = true,
            SortBy = DishSortField.Popularity,
            Descending = true,
            Limit = limit is > 0 ? limit : 12
        };

        _logger.LogInformation("Retrieving menu. Category: {Category}, Featured only: {FeaturedOnly}, Limit: {Limit}", category, featuredOnly ?? false, options.Limit);

        var dishes = await _dishRepository.GetAsync(options, cancellationToken);
        var filtered = featuredOnly == true
            ? dishes.Where(d => d.IsFeatured).ToList()
            : dishes.ToList();

        var responses = filtered.Select(d => d.ToResponse()).ToList();
        return Ok(new { items = responses, totalCount = responses.Count });
    }
}
