using Feane.Contracts.Reservations;
using FeaneMVC.Application.Queries.Sessions;
using FeaneMVC.Clients;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace FeaneMVC.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ReservationController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IReservationServiceClient _reservationServiceClient;

        public ReservationController(IMediator mediator, IReservationServiceClient reservationServiceClient)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _reservationServiceClient = reservationServiceClient ?? throw new ArgumentNullException(nameof(reservationServiceClient));
        }

        public async Task<IActionResult> Book()
        {
            var userId = await _mediator.Send(new GetCurrentUserIdQuery());
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Authentication", "Account", new { returnUrl = Url.Action(nameof(Book)) });
            }

            var defaultRequest = new CreateReservationRequest
            {
                ReservationDateTime = DateTime.Now.AddHours(2),
                NumberOfPeople = 2
            };

            return View(defaultRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReservationProcess(CreateReservationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View("Book", request);
            }

            var userId = await _mediator.Send(new GetCurrentUserIdQuery());
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Authentication", "Account", new { returnUrl = Url.Action(nameof(Book)) });
            }

            try
            {
                var reservation = await _reservationServiceClient.CreateAsync(userId, request);
                if (reservation is not null)
                {
                    ViewData["SuccessMessage"] = "Столик успешно забронирован.";
                    ModelState.Clear();
                    return View("Book", new CreateReservationRequest
                    {
                        ReservationDateTime = request.ReservationDateTime,
                        NumberOfPeople = request.NumberOfPeople,
                        BudgetPerGuest = request.BudgetPerGuest
                    });
                }

                ModelState.AddModelError(string.Empty, "Не удалось создать резервацию.");
                return View("Book", request);
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, "Сервис резерваций недоступен. Попробуйте позже.");
                return View("Book", request);
            }
        }
    }
}
