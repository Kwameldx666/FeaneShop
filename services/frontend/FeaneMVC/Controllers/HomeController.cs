using FeaneMVC.Application.Commands.Sessions;
using FeaneMVC.Application.Queries.Dishes;
using FeaneMVC.Application.Queries.Sessions;
using FeaneMVC.Application.Queries.Users;
using FeaneMVC.Contracts.Dishes;
using FeaneMVC.Extenstions;
using Microsoft.AspNetCore.Mvc;

namespace FeaneMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MediatR.IMediator _mediator;

    public HomeController(ILogger<HomeController> logger, MediatR.IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
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

        var dishDtos = await _mediator.Send(new GetAllDishesQuery());
        var dishes = dishDtos?.ToResponseCollection() ?? Enumerable.Empty<DishResponse>();

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
        var userId = await _mediator.Send(new GetCurrentUserIdQuery());
        if (userId == Guid.Empty)
        {
            return;
        }

        var user = await _mediator.Send(new GetUserProfileByIdQuery(userId));
        if (user?.Data?.User == null)
        {
            return;
        }

        await _mediator.Send(new SetSessionValueCommand("UserId", user.Data.User.Id.ToString()));
        await _mediator.Send(new SetSessionValueCommand("UserRole", user.Data.User.Roles.ToString()));
    }
}
