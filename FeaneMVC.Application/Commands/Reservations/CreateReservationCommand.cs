using System;
using FeaneMVC.Application.DTOs.Reservations;
using FeaneMVC.Domain.ValueObjects;
using MediatR;

namespace FeaneMVC.Application.Commands.Reservations;

public sealed record CreateReservationCommand(CreateReservationDto Reservation, Guid UserId) : IRequest<OperationResult<ReservationDto>>;
