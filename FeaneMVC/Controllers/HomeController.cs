using FeaneMVC.Application.Commands.Sessions;
using FeaneMVC.Application.Queries.Sessions;
using FeaneMVC.Application.Queries.Users;
using FeaneMVC.Clients;
using Feane.Contracts.Dishes;
using System.Linq;
using System;
using Microsoft.AspNetCore.Mvc;

namespace FeaneMVC.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly MediatR.IMediator _mediator;
    private readonly IMenuServiceClient _menuServiceClient;

    public HomeController(ILogger<HomeController> logger, MediatR.IMediator mediator, IMenuServiceClient menuServiceClient)
    {
        _logger = logger;
        _mediator = mediator;
        _menuServiceClient = menuServiceClient;
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

        IReadOnlyCollection<DishResponse> dishes;

        try
        {
            dishes = await _menuServiceClient.GetDishesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to retrieve dishes from the menu service.");
            dishes = Array.Empty<DishResponse>();
            ViewBag.Message = "Сервис меню временно недоступен.";
        }

        ViewBag.DishMenu = dishes;

        if (!dishes.Any())
        {
            if (ViewBag.Message == null)
            {
                ViewBag.Message = "No dishes available.";
            }
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
