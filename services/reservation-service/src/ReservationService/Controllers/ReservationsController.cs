using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using ReservationService.Application.DTOs;
using ReservationService.Application.Interfaces;
using ReservationService.Application.Mappers;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Enums;

namespace ReservationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private const int MaxPageSize = 200;
    private readonly ILogger<ReservationsController> _logger;
    private readonly IReservationRepository _repository;

    public ReservationsController(IReservationRepository repository, ILogger<ReservationsController> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> GetReservations(
        [FromQuery] Guid? userId,
        [FromQuery] string? email,
        [FromQuery] string? status,
        [FromQuery] bool? upcoming,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? sort,
        [FromQuery] bool? desc,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        try
        {
            // Извлекаем userId из JWT токена или заголовков
            var authenticatedUserId = ExtractUserIdFromContext();

            // Если userId передан в query, проверяем что он совпадает с аутентифицированным пользователем
            // (для безопасности - пользователь может видеть только свои резервации)
            if (authenticatedUserId.HasValue)
            {
                if (userId.HasValue && userId.Value != authenticatedUserId.Value)
                {
                    _logger.LogWarning("User {AuthUserId} attempted to access reservations of {RequestedUserId}",
                        authenticatedUserId, userId);
                    return Forbid();
                }

                userId = authenticatedUserId;
            }

            _logger.LogInformation("GetReservations called with UserId={UserId}, Email={Email}, Status={Status}",
                userId, email, status);

            // Если userId не определен, возвращаем пустой список (требуется аутентификация)
            if (!userId.HasValue)
            {
                _logger.LogWarning("GetReservations called without userId - authentication required");
                return Ok(new
                {
                    success = true,
                    message = "Пожалуйста, войдите в систему для просмотра резерваций.",
                    items = Array.Empty<object>(),
                    totalCount = 0,
                    page = page ?? 1,
                    pageSize = pageSize ?? 25
                });
            }

            var options = new ReservationQueryOptions
            {
                UserId = userId,
                Email = NormalizeEmail(email),
                Status = ParseStatus(status),
                UpcomingOnly = upcoming ?? false,
                FromDate = NormalizeDate(from),
                ToDate = NormalizeDate(to),
                Sort = sort,
                Descending = desc ?? ShouldSortDescending(sort),
                Page = page is > 0 ? page.Value : 1,
                PageSize = pageSize is > 0 ? Math.Min(pageSize.Value, MaxPageSize) : 25
            };

            var items = await _repository.GetAsync(options, cancellationToken);
            var total = await _repository.CountAsync(options, cancellationToken);
            var responses = items.Select(ReservationMapper.ToResponse).ToList();

            _logger.LogInformation("GetReservations returned {Count} items out of {Total} total",
                responses.Count, total);

            return Ok(new
            {
                success = true,
                items = responses,
                totalCount = total,
                page = options.Page,
                pageSize = options.PageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservations with UserId={UserId}, Email={Email}", userId, email);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "Unable to retrieve reservations at the moment.",
                error = ex.Message,
                items = Array.Empty<object>(),
                totalCount = 0,
                page = page ?? 1,
                pageSize = pageSize ?? 25
            });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReservation(Guid id, CancellationToken cancellationToken)
    {
        var reservation = await _repository.GetByIdAsync(id, cancellationToken);
        if (reservation == null) return NotFound(new { success = false, message = "Reservation not found." });

        return Ok(reservation.ToResponse());
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationCreateRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var reservation = MapToEntity(request);
        AttachUserContext(reservation);

        try
        {
            var created = await _repository.AddAsync(reservation, cancellationToken);
            var response = created.ToResponse();

            return CreatedAtAction(nameof(GetReservation), new { id = response.Id }, new
            {
                success = true,
                message = "Reservation created successfully.",
                item = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create reservation for {@Request}", request);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                success = false,
                message = "Unable to create reservation at the moment."
            });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelReservation(Guid id, CancellationToken cancellationToken)
    {
        var success = await _repository.UpdateStatusAsync(id, ReservationStatus.Cancelled, cancellationToken);
        if (!success) return NotFound(new { success = false, message = "Reservation not found." });

        return Ok(new { success = true, message = "Reservation cancelled." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReservation(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        if (!deleted) return NotFound(new { success = false, message = "Reservation not found." });

        return Ok(new { success = true, message = "Reservation deleted." });
    }

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        return email.Trim().ToLower(CultureInfo.InvariantCulture);
    }

    private static ReservationStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status)) return null;

        if (Enum.TryParse<ReservationStatus>(status, true, out var parsed)) return parsed;

        return null;
    }

    private static bool ShouldSortDescending(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort)) return true;

        return sort.StartsWith("-", StringComparison.Ordinal) ||
               sort.EndsWith("Desc", StringComparison.OrdinalIgnoreCase);
    }

    private static DateTime? NormalizeDate(DateTime? value)
    {
        if (!value.HasValue) return null;

        return DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
    }

    private Reservation MapToEntity(ReservationCreateRequest request)
    {
        var normalizedDate = NormalizeReservationDate(request.ReservationDateTime);
        var budgetPerGuest = Math.Max(0, request.BudgetPerGuest ?? 0);
        var estimatedTotal = Math.Round(budgetPerGuest * Math.Max(1, request.NumberOfPeople), 2,
            MidpointRounding.AwayFromZero);

        return new Reservation
        {
            UserId = request.UserId,
            CustomerName = request.CustomerName.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            UserEmail = request.UserEmail.Trim(),
            NumberOfPeople = request.NumberOfPeople,
            ReservationDate = normalizedDate,
            Occasion = string.IsNullOrWhiteSpace(request.Occasion) ? null : request.Occasion.Trim(),
            SeatingPreference = string.IsNullOrWhiteSpace(request.SeatingPreference)
                ? null
                : request.SeatingPreference.Trim(),
            SpecialRequests =
                string.IsNullOrWhiteSpace(request.SpecialRequests) ? null : request.SpecialRequests.Trim(),
            BudgetPerGuest = Math.Round(budgetPerGuest, 2, MidpointRounding.AwayFromZero),
            EstimatedTotal = estimatedTotal,
            Status = ReservationStatus.Pending
        };
    }

    private Guid? ExtractUserIdFromContext()
    {
        // Пытаемся извлечь из JWT токена
        if (Request.HttpContext.User?.Identity?.IsAuthenticated == true)
        {
            var userIdClaim =
                Request.HttpContext.User.FindFirst(
                    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier") ??
                Request.HttpContext.User.FindFirst("sub") ??
                Request.HttpContext.User.FindFirst("user_id") ??
                Request.HttpContext.User.FindFirst("userId");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                _logger.LogDebug("UserId extracted from JWT: {UserId}", userId);
                return userId;
            }
        }

        // Пытаемся извлечь из заголовка X-User-Id
        if (Request.Headers.TryGetValue("X-User-Id", out var headerUserId))
            if (Guid.TryParse(headerUserId.ToString(), out var headerGuid))
            {
                _logger.LogDebug("UserId extracted from X-User-Id header: {UserId}", headerGuid);
                return headerGuid;
            }

        _logger.LogDebug("UserId not found in request context");
        return null;
    }

    private void AttachUserContext(Reservation reservation)
    {
        var userId = ExtractUserIdFromContext();
        if (userId.HasValue) reservation.UserId = userId;
    }

    private static DateTime NormalizeReservationDate(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc) return value;

        if (value.Kind == DateTimeKind.Unspecified)
        {
            var assumedLocal = DateTime.SpecifyKind(value, DateTimeKind.Local);
            return assumedLocal.ToUniversalTime();
        }

        return value.ToUniversalTime();
    }
}