using System;
using System.Threading;
using System.Threading.Tasks;
using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Application.DTOs.Reservations;
using FeaneMVC.Application.Mapping;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Reservations.Handlers;

public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, OperationResult<ReservationDto>>
{
    private readonly IReservation _reservationService;

    public CreateReservationCommandHandler(IReservation reservationService)
    {
        _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
    }

    public Task<OperationResult<ReservationDto>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var reservationEntity = request.Reservation.ToEntity();
        var result = _reservationService.CreateReservation(reservationEntity, request.UserId);

        if (!result.Status || result.Data is null)
        {
            return Task.FromResult(OperationResult<ReservationDto>.Failure(result.Message ?? "Failed to create reservation."));
        }

        return Task.FromResult(OperationResult<ReservationDto>.Success(result.Data.ToDto(), result.Message));
    }
}
