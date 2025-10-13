using System.Linq;
using Feane.Contracts.Reservations;
using FeaneMVC.Clients.Reservations;
using FeaneMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeaneMVC.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationApiClient _reservationApiClient;
        private readonly IUserSessionAccessor _userSessionAccessor;

        public ReservationController(
            IReservationApiClient reservationApiClient,
            IUserSessionAccessor userSessionAccessor)
        {
            _reservationApiClient = reservationApiClient;
            _userSessionAccessor = userSessionAccessor;
        }

        public async Task<IActionResult> Book()
        {
            await _userSessionAccessor.GetOrCreateUserIdAsync(HttpContext);

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

            var userId = await _userSessionAccessor.GetOrCreateUserIdAsync(HttpContext);
            request.UserId = userId;

            try
            {
                var history = await _reservationApiClient.CreateAsync(request);
                ViewData["SuccessMessage"] = history.StatusMessage ?? "Столик успешно забронирован.";
                history.Items = history.Items.Take(5).ToList();
                ViewData["ReservationHistory"] = history;
                ModelState.Clear();
                return View("Book", new CreateReservationRequest
                {
                    ReservationDateTime = request.ReservationDateTime,
                    NumberOfPeople = request.NumberOfPeople,
                    BudgetPerGuest = request.BudgetPerGuest
                });
            }
            catch (Exception exception)
            {
                ModelState.AddModelError(string.Empty, exception.Message);
                return View("Book", request);
            }
        }
    }
}
