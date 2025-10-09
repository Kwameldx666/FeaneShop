using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Application.DTOs.Reservations;
using FeaneMVC.Application.Mapping;
using MediatR;

namespace FeaneMVC.Application.Queries.Reservations.Handlers;

public class GetReservationsByUserIdQueryHandler : IRequestHandler<GetReservationsByUserIdQuery, IEnumerable<ReservationDto>>
{
    private readonly IReservation _reservationService;

    public GetReservationsByUserIdQueryHandler(IReservation reservationService)
    {
        _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
    }

    public Task<IEnumerable<ReservationDto>> Handle(GetReservationsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var reservations = _reservationService.GetReservationsByUserId(request.UserId) ?? Enumerable.Empty<Domain.Entities.Reservation>();
        return Task.FromResult(reservations.ToDtoCollection());
    }
}
