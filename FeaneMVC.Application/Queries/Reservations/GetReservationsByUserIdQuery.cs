using System;
using System.Collections.Generic;
using FeaneMVC.Application.DTOs.Reservations;
using MediatR;

namespace FeaneMVC.Application.Queries.Reservations;

public sealed record GetReservationsByUserIdQuery(Guid UserId) : IRequest<IEnumerable<ReservationDto>>;
