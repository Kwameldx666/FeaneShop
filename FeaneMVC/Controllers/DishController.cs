using Feane.Contracts.Dishes;
using FeaneMVC.Attributes;
using FeaneMVC.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace FeaneMVC.Controllers
{
    [ServiceFilter(typeof(AdminOrModeratorModeAttribute))]
    public class DishController : Controller
    {
        private readonly IMenuServiceClient _menuServiceClient;
        private readonly ILogger<DishController> _logger;

        public DishController(IMenuServiceClient menuServiceClient, ILogger<DishController> logger)
        {
            _menuServiceClient = menuServiceClient;
            _logger = logger;
        }

        public async Task<ActionResult> Index()
        {
            List<DishResponse> dishes;

            try
            {
                dishes = (await _menuServiceClient.GetDishesAsync()).ToList();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to load dishes from the menu service.");
                TempData["Error"] = "Menu service is unavailable. Please try again later.";
                dishes = new List<DishResponse>();
            }

            if (!dishes.Any())
            {
                if (ViewBag.Message == null)
                {
                    ViewBag.Message = "No dishes available. Please add some dishes.";
                }
            }

            return View(dishes);
        }

        public async Task<ActionResult> Details(Guid id)
        {
            try
            {
                var dish = await _menuServiceClient.GetDishAsync(id);
                if (dish is null)
                {
                    TempData["Error"] = "Dish not found.";
                    return RedirectToAction("Index");
                }

                return View(dish);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to load dish {DishId}", id);
                TempData["Error"] = "Failed to load dish details.";
                return RedirectToAction("Index");
            }
        }

        public ActionResult AddDish()
        {
            return View(new CreateDishRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDish(CreateDishRequest request, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            await UploadImageAsync(request, imageFile);

            try
            {
                var dish = await _menuServiceClient.CreateDishAsync(request);
                if (dish is null)
                {
                    ModelState.AddModelError(string.Empty, "Failed to add the dish. Please try again.");
                    return View(request);
                }

                TempData["Message"] = "Dish added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to create dish {DishName}", request.Name);
                ModelState.AddModelError(string.Empty, "Menu service is unavailable. Please try again later.");
                return View(request);
            }
        }

        public async Task<ActionResult> EditDish(Guid id)
        {
            try
            {
                var dish = await _menuServiceClient.GetDishAsync(id);
                if (dish is null)
                {
                    TempData["Error"] = "Dish not found.";
                    return RedirectToAction("Index");
                }

                var updateRequest = new UpdateDishRequest
                {
                    Id = dish.Id,
                    Name = dish.Name,
                    Description = dish.Description,
                    Price = dish.Price,
                    Category = dish.Category,
                    ImageUrl = dish.ImageUrl
                };

                return View(updateRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to load dish {DishId} for edit", id);
                TempData["Error"] = "Failed to load dish details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDish(UpdateDishRequest request, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            await UploadImageAsync(request, imageFile);

            try
            {
                var dish = await _menuServiceClient.UpdateDishAsync(request.Id, request);
                if (dish is null)
                {
                    ModelState.AddModelError(string.Empty, "Failed to update the dish. Please try again.");
                    return View(request);
                }

                TempData["Message"] = "Dish updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to update dish {DishId}", request.Id);
                ModelState.AddModelError(string.Empty, "Menu service is unavailable. Please try again later.");
                return View(request);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDish(Guid id)
        {
            try
            {
                var removed = await _menuServiceClient.DeleteDishAsync(id);
                return Json(new
                {
                    status = removed,
                    message = removed ? "Dish deleted successfully." : "Failed to delete the dish."
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to delete dish {DishId}", id);
                return Json(new
                {
                    status = false,
                    message = "Menu service is unavailable. Please try again later."
                });
            }
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

            try
            {
                var (created, skipped) = await _menuServiceClient.SeedAsync(count);

                if (created > 0 || skipped > 0)
                {
                    TempData["Message"] = $"Generated {created} dishes. Skipped {skipped} duplicates.";
                }
                else
                {
                    TempData["Error"] = "Failed to generate sample dishes.";
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to seed sample dishes");
                TempData["Error"] = "Menu service is unavailable. Please try again later.";
            }

            return RedirectToAction("Index");
        }

        private static async Task UploadImageAsync(CreateDishRequest request, IFormFile? imageFile)
        {
            if (imageFile is null || imageFile.Length == 0)
            {
                return;
            }

            var fileName = Path.GetFileName(imageFile.FileName);
            var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
            Directory.CreateDirectory(imagesFolder);

            var filePath = Path.Combine(imagesFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            request.ImageUrl = $"/Images/{fileName}";
        }

        private static async Task UploadImageAsync(UpdateDishRequest request, IFormFile? imageFile)
        {
            var createRequest = new CreateDishRequest
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
                ImageUrl = request.ImageUrl
            };

            await UploadImageAsync(createRequest, imageFile);
            request.ImageUrl = createRequest.ImageUrl;
        }
    }
}
