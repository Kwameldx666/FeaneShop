using System;
using System.Threading.Tasks;
using FeaneMVC.Application.Commands.Reservations;
using FeaneMVC.Application.Queries.Sessions;
using FeaneMVC.Contracts.Reservations;
using FeaneMVC.Extenstions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace FeaneMVC.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ReservationController : Controller
    {
        private readonly IMediator _mediator;

        public ReservationController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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

            var reservationResponse = await _mediator.Send(request.ToCommand(userId));

            if (reservationResponse.Status)
            {
                ViewData["SuccessMessage"] = string.IsNullOrWhiteSpace(reservationResponse.Message)
                    ? "Столик успешно забронирован."
                    : reservationResponse.Message;

                ModelState.Clear();
                return View("Book", new CreateReservationRequest
                {
                    ReservationDateTime = request.ReservationDateTime,
                    NumberOfPeople = request.NumberOfPeople,
                    BudgetPerGuest = request.BudgetPerGuest
                });
            }

            ModelState.AddModelError(string.Empty, reservationResponse.Message ?? "Не удалось создать резервацию.");
            return View("Book", request);
        }
    }
}
