using System.Linq;
using Feane.Contracts.Reservations;
using FeaneMVC.Application.Commands.Reservations;
using FeaneMVC.Application.DTOs.Reservations;
using FeaneMVC.Application.Queries.Reservations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Feane.ReservationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReservationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservationHistoryItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReservationHistoryItem>> CreateAsync([FromBody] CreateReservationRequest request)
    {
        if (request.UserId == Guid.Empty)
        {
            return BadRequest("User identifier is required to create a reservation.");
        }

        var reservationDto = new CreateReservationDto
        {
            CustomerName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            UserEmail = request.UserEmail,
            NumberOfPeople = request.NumberOfPeople,
            ReservationDate = request.ReservationDateTime,
            Occasion = request.Occasion,
            SeatingPreference = request.SeatingPreference,
            SpecialRequests = request.SpecialRequests,
            Amount = request.BudgetPerGuest * request.NumberOfPeople
        };

        var result = await _mediator.Send(new CreateReservationCommand(reservationDto, request.UserId));
        if (!result.Status || result.Data is null)
        {
            return BadRequest(result.Message ?? "Failed to create reservation");
        }

        var reservation = result.Data;
        var historyItem = new ReservationHistoryItem
        {
            ReservationId = reservation.Id,
            ReservationDate = reservation.ReservationDate,
            CustomerName = reservation.CustomerName,
            NumberOfPeople = reservation.NumberOfPeople,
            Status = reservation.Status.ToString(),
            Amount = reservation.Amount,
            Occasion = reservation.Occasion,
            SeatingPreference = reservation.SeatingPreference,
            SpecialRequests = reservation.SpecialRequests,
            UpdatedAt = reservation.UpdatedAt
        };

        return CreatedAtAction(nameof(GetByUserAsync), new { userId = reservation.UserId }, historyItem);
    }

    [HttpGet("user/{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ReservationHistoryItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationHistoryItem>>> GetByUserAsync(Guid userId)
    {
        var reservations = await _mediator.Send(new GetReservationsByUserIdQuery(userId));
        var history = reservations.Select(reservation => new ReservationHistoryItem
        {
            ReservationId = reservation.Id,
            ReservationDate = reservation.ReservationDate,
            CustomerName = reservation.CustomerName,
            NumberOfPeople = reservation.NumberOfPeople,
            Status = reservation.Status.ToString(),
            Amount = reservation.Amount,
            Occasion = reservation.Occasion,
            SeatingPreference = reservation.SeatingPreference,
            SpecialRequests = reservation.SpecialRequests,
            UpdatedAt = reservation.UpdatedAt
        });

        return Ok(history);
    }
}
