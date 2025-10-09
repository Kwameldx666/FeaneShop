using FeaneMVC.Application.Commands.Dishes;
using FeaneMVC.Application.Queries.Dishes;
using FeaneMVC.Attributes;
using FeaneMVC.Contracts.Dishes;
using FeaneMVC.Extenstions;
using Microsoft.AspNetCore.Mvc;

namespace FeaneMVC.Controllers
{
    [ServiceFilter(typeof(AdminOrModeratorModeAttribute))]
    public class DishController : Controller
    {
        private readonly MediatR.IMediator _mediator;

        public DishController(MediatR.IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index()
        {
            var dishDtos = await _mediator.Send(new GetAllDishesQuery());
            var dishes = dishDtos?.ToResponseCollection().ToList() ?? new List<DishResponse>();

            if (!dishes.Any())
            {
                ViewBag.Message = "No dishes available. Please add some dishes.";
                return View(dishes);
            }

            return View(dishes);
        }

        public async Task<ActionResult> Details(Guid id)
        {
            var dishResult = await _mediator.Send(new GetDishByIdQuery(id));
            if (dishResult == null || !dishResult.Status || dishResult.Data == null)
            {
                TempData["Error"] = "Dish not found.";
                return RedirectToAction("Index");
            }

            return View(dishResult.Data.ToResponse());
        }

        public ActionResult AddDish()
        {
            return View(new CreateDishRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDish(CreateDishRequest request, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                request.ImageUrl = $"/Images/{fileName}";
            }

            var dishResponse = await _mediator.Send(request.ToCommand());
            if (dishResponse.Status && dishResponse.Data != null)
            {
                TempData["Message"] = "Dish added successfully!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, dishResponse.Message ?? "Failed to add the dish. Please try again.");
            return View(request);
        }

        public async Task<ActionResult> EditDish(Guid id)
        {
            var dishResult = await _mediator.Send(new GetDishByIdQuery(id));
            if (dishResult == null || !dishResult.Status || dishResult.Data == null)
            {
                TempData["Error"] = "Dish not found.";
                return RedirectToAction("Index");
            }

            var dish = dishResult.Data.ToResponse();
            return View(dish.ToUpdateRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDish(UpdateDishRequest request, IFormFile imageFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await imageFile.CopyToAsync(stream);

                    request.ImageUrl = $"/Images/{fileName}";
                }

                var dishResponse = await _mediator.Send(request.ToCommand());
                if (dishResponse.Status)
                {
                    TempData["Message"] = "Dish updated successfully!";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, dishResponse.Message ?? "Failed to update the dish. Please try again.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating dish: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the dish.");
            }

            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDish(Guid id)
        {
            var dishResponse = await _mediator.Send(new DeleteDishCommand(id));
            return Json(new
            {
                status = dishResponse.Status,
                message = dishResponse.Message ?? (dishResponse.Status ? "Dish deleted successfully." : "Failed to delete the dish.")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedSampleData(int count)
        {
            if (count <= 0 || count > 100)
            {
                TempData["Error"] = "Count must be between 1 and 100.";
                return RedirectToAction("Index");
            }

            var seedResponse = await _mediator.Send(new SeedSampleDishesCommand(count));

            if (seedResponse.Status && seedResponse.Data != null)
            {
                TempData["Message"] = $"Generated {seedResponse.Data.CreatedCount} dishes. Skipped {seedResponse.Data.SkippedCount} duplicates.";
            }
            else
            {
                TempData["Error"] = seedResponse.Message ?? "Failed to generate sample dishes.";
            }

            return RedirectToAction("Index");
        }
    }
}
