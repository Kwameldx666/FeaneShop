using System;
using System.Threading;
using System.Threading.Tasks;
using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Application.DTOs.Reservations;
using FeaneMVC.Application.Mapping;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Reservations.Handlers;

public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand, OperationResult<ReservationDto>>
{
    private readonly IReservation _reservationService;

    public CancelReservationCommandHandler(IReservation reservationService)
    {
        _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
    }

    public Task<OperationResult<ReservationDto>> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = _reservationService.GetReservationById(request.ReservationId);

        if (!reservation.Status || reservation.Data is null)
        {
            return Task.FromResult(OperationResult<ReservationDto>.Failure(reservation.Message ?? "Reservation not found."));
        }

        if (reservation.Data.UserId != request.UserId)
        {
            return Task.FromResult(OperationResult<ReservationDto>.Failure("You can only cancel your own reservations."));
        }

        var cancellationResult = _reservationService.CancelReservation(request.ReservationId);

        if (!cancellationResult.Status || cancellationResult.Data is null)
        {
            return Task.FromResult(OperationResult<ReservationDto>.Failure(cancellationResult.Message ?? "Failed to cancel reservation."));
        }

        return Task.FromResult(OperationResult<ReservationDto>.Success(cancellationResult.Data.ToDto(), cancellationResult.Message));
    }
}
