using System.Linq;
using Feane.Contracts.Dishes;
using FeaneMVC.Clients.Menu;
using FeaneMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeaneMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMenuApiClient _menuApiClient;
    private readonly IUserSessionAccessor _userSessionAccessor;

    public HomeController(
        ILogger<HomeController> logger,
        IMenuApiClient menuApiClient,
        IUserSessionAccessor userSessionAccessor)
    {
        _logger = logger;
        _menuApiClient = menuApiClient;
        _userSessionAccessor = userSessionAccessor;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            await EnsureUserSessionAsync();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to hydrate user session");
        }

        var dishes = await _menuApiClient.GetAllAsync();

        ViewBag.DishMenu = dishes;

        if (!dishes.Any())
        {
            ViewBag.Message = "No dishes available.";
        }

        return View();
    }

    public IActionResult About() => View();

    public IActionResult Book() => View();

    public IActionResult Menu() => View();

    private async Task EnsureUserSessionAsync()
    {
        await _userSessionAccessor.GetOrCreateUserIdAsync(HttpContext);
    }
}
