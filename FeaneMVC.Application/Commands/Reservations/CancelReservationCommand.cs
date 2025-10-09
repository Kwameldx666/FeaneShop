using System;
using FeaneMVC.Application.DTOs.Reservations;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Reservations;

public sealed record CancelReservationCommand(Guid ReservationId, Guid UserId) : IRequest<OperationResult<ReservationDto>>;
