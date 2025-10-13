using System.Linq;
using Feane.Contracts.Dishes;
using FeaneMVC.Clients.Menu;
using Microsoft.AspNetCore.Mvc;

namespace FeaneMVC.Controllers
{
    public class DishController : Controller
    {
        private readonly IMenuApiClient _menuApiClient;

        public DishController(IMenuApiClient menuApiClient)
        {
            _menuApiClient = menuApiClient;
        }

        public async Task<ActionResult> Index()
        {
            var dishes = (await _menuApiClient.GetAllAsync()).ToList();

            if (!dishes.Any())
            {
                ViewBag.Message = "No dishes available. Please add some dishes.";
                return View(dishes);
            }

            return View(dishes);
        }

        public async Task<ActionResult> Details(Guid id)
        {
            var dish = await _menuApiClient.GetByIdAsync(id);
            if (dish == null)
            {
                TempData["Error"] = "Dish not found.";
                return RedirectToAction("Index");
            }

            return View(dish);
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

            var dishResponse = await _menuApiClient.CreateAsync(request);
            if (dishResponse != null)
            {
                TempData["Message"] = "Dish added successfully!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, "Failed to add the dish. Please try again.");
            return View(request);
        }

        public async Task<ActionResult> EditDish(Guid id)
        {
            var dish = await _menuApiClient.GetByIdAsync(id);
            if (dish == null)
            {
                TempData["Error"] = "Dish not found.";
                return RedirectToAction("Index");
            }

            return View(new UpdateDishRequest
            {
                Id = dish.Id,
                Name = dish.Name,
                Description = dish.Description,
                Price = dish.Price,
                Category = dish.Category,
                ImageUrl = dish.ImageUrl
            });
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

                var updateSucceeded = await _menuApiClient.UpdateAsync(request);
                if (updateSucceeded)
                {
                    TempData["Message"] = "Dish updated successfully!";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, "Failed to update the dish. Please try again.");
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
            await _menuApiClient.DeleteAsync(id);
            return Json(new
            {
                status = true,
                message = "Dish deleted successfully."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedSampleData(int count)
        {
            TempData["Error"] = "Automatic seeding is not supported in the gateway. Use the menu microservice directly.";
            return RedirectToAction("Index");
        }
    }
}
