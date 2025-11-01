using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Mappers;
using ProductService.Domain.Entities;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DishesController : ControllerBase
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;
    private const int MaxImageSizeBytes = 2 * 1024 * 1024; // 2 MB

    private readonly IDishRepository _dishRepository;
    private readonly ILogger<DishesController> _logger;

    public DishesController(IDishRepository dishRepository, ILogger<DishesController> logger)
    {
        _dishRepository = dishRepository ?? throw new ArgumentNullException(nameof(dishRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetDishes([FromQuery] string? category, [FromQuery] string? search,
        [FromQuery] bool? availableOnly,
        [FromQuery] string? sort, [FromQuery] bool? desc, [FromQuery] int? page, [FromQuery] int? pageSize,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var options = new DishQueryOptions
        {
            Category = category,
            Search = search,
            AvailableOnly = availableOnly ?? false,
            SortBy = ParseSortField(sort),
            Descending = desc ?? ShouldSortDescending(sort),
            Limit = limit > 0 ? limit : null,
            Page = page is > 0 ? page : null,
            PageSize = pageSize is > 0 ? Math.Min(pageSize.Value, MaxPageSize) : DefaultPageSize
        };

        var dishes = await _dishRepository.GetAsync(options, cancellationToken);
        var total = await _dishRepository.CountAsync(options, cancellationToken);
        var responses = dishes.Select(d => d.ToResponse()).ToList();

        return Ok(new
        {
            items = responses,
            totalCount = total,
            page = options.Page ?? 1,
            pageSize = options.PageSize ?? DefaultPageSize
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDish(Guid id, CancellationToken cancellationToken)
    {
        var dish = await _dishRepository.GetByIdAsync(id, cancellationToken);
        if (dish == null) return NotFound(new { success = false, message = "Dish not found." });

        return Ok(dish.ToResponse());
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var categories = await _dishRepository.GetCategoriesAsync(cancellationToken);
        return Ok(new { items = categories });
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateDish([FromForm] DishUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        string? imageBase64 = null;
        string? imageMimeType = null;

        try
        {
            (imageBase64, imageMimeType) = await ReadImageAsync(request.ImageFile, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogWarning(exception, "Image validation failed while creating a dish");
            return BadRequest(new { success = false, message = exception.Message });
        }

        var dish = new Dish
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = Math.Round(request.Price, 2, MidpointRounding.AwayFromZero),
            Category = request.Category.Trim().ToLowerInvariant(),
            IsAvailable = request.IsAvailable,
            IsFeatured = request.IsFeatured,
            PopularityScore = request.PopularityScore,
            ImageBase64 = imageBase64,
            ImageMimeType = imageMimeType
        };

        var created = await _dishRepository.AddAsync(dish, cancellationToken);
        var response = created.ToResponse();

        return CreatedAtAction(nameof(GetDish), new { id = response.Id }, new
        {
            success = true,
            message = "Dish created successfully.",
            item = response
        });
    }

    [HttpPost("{id:guid}")]
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateDish(Guid id, [FromForm] DishUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var existing = await _dishRepository.GetByIdAsync(id, cancellationToken);
        if (existing == null) return NotFound(new { success = false, message = "Dish not found." });

        existing.Name = request.Name.Trim();
        existing.Description = request.Description.Trim();
        existing.Price = Math.Round(request.Price, 2, MidpointRounding.AwayFromZero);
        existing.Category = request.Category.Trim().ToLowerInvariant();
        existing.IsAvailable = request.IsAvailable;
        existing.IsFeatured = request.IsFeatured;
        existing.PopularityScore = request.PopularityScore;

        if (request.ImageFile != null)
            try
            {
                var (imageBase64, imageMimeType) = await ReadImageAsync(request.ImageFile, cancellationToken);
                if (!string.IsNullOrWhiteSpace(imageBase64))
                {
                    existing.ImageBase64 = imageBase64;
                    existing.ImageMimeType = imageMimeType;
                }
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogWarning(exception, "Image validation failed while updating dish {DishId}", id);
                return BadRequest(new { success = false, message = exception.Message });
            }

        var updated = await _dishRepository.UpdateAsync(existing, cancellationToken);
        if (updated == null)
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { success = false, message = "Unable to update dish." });

        var response = updated.ToResponse();
        return Ok(new { success = true, message = "Dish updated successfully.", item = response });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDish(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _dishRepository.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { success = false, message = "Dish not found." });

        return Ok(new { success = true, message = "Dish deleted successfully." });
    }

    private async Task<(string? Base64, string? MimeType)> ReadImageAsync(IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return (null, null);

        if (file.Length > MaxImageSizeBytes)
            throw new InvalidOperationException(
                $"Image size exceeds the maximum allowed size of {MaxImageSizeBytes / 1024 / 1024} MB.");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var base64 = Convert.ToBase64String(memoryStream.ToArray());
        var mimeType = string.IsNullOrWhiteSpace(file.ContentType) ? "image/png" : file.ContentType;
        return (base64, mimeType);
    }

    private static DishSortField ParseSortField(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return DishSortField.CreatedAt;

        return sort.Trim().ToLowerInvariant() switch
        {
            "name" => DishSortField.Name,
            "price" => DishSortField.Price,
            "updated" => DishSortField.UpdatedAt,
            "popularity" => DishSortField.Popularity,
            _ => DishSortField.CreatedAt
        };
    }

    private static bool ShouldSortDescending(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return true;

        return sort.Trim().ToLowerInvariant() switch
        {
            "name" => false,
            "price" => false,
            _ => true
        };
    }
}