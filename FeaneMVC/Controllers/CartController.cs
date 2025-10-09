using System;
using FeaneMVC.Application.Commands.Carts;
using FeaneMVC.Application.Queries.Carts;
using FeaneMVC.Application.Queries.Sessions;
using FeaneMVC.Application.Queries.Users;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Enums;
using FeaneMVC.Domain.ValueObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;

namespace FeaneMVC.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CartController : Controller
    {
        private readonly IMediator _mediator;

        public CartController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        public async Task<IActionResult> Add(Guid dishId, string dishName, decimal dishPrice, int quantity)
        {
            if (dishId == Guid.Empty || string.IsNullOrWhiteSpace(dishName) || dishPrice <= 0 || quantity <= 0)
            {
                return Json(new { success = false, message = "Invalid input data." });
            }

            var (userId, userResponse) = await GetCurrentUserAsync();
            if (userResponse == null)
            {
                var redirectUrl = Url.Action("Authentication", "Account", new { returnUrl = Url.Action(nameof(Cart)) });
                return Json(new { success = false, redirect = redirectUrl, message = "Please sign in to manage your cart." });
            }

            var cartItem = new CartItem
            {
                Name = dishName,
                Price = dishPrice,
                DishId = dishId,
                Quantity = quantity,
                TotalPrice = dishPrice * quantity,
                UserId = userId
            };

            var applyVipDiscount = userResponse.Data!.User!.Roles == Role.VIP;
            await _mediator.Send(new AddCartItemCommand(userId, cartItem, applyVipDiscount));

            return Json(new
            {
                success = true,
                message = $"'{cartItem.Name}' was added to your cart."
            });
        }

        public async Task<IActionResult> Cart()
        {
            var (userId, userResponse) = await GetCurrentUserAsync();
            if (userResponse == null)
            {
                return RedirectToAction("Authentication", "Account", new { returnUrl = Url.Action(nameof(Cart)) });
            }

            var userCart = await _mediator.Send(new GetCartQuery(userId));

            return View(userCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid dishId)
        {
            var (userId, userResponse) = await GetCurrentUserAsync();
            if (userResponse == null)
            {
                return RedirectToAction("Authentication", "Account", new { returnUrl = Url.Action(nameof(Cart)) });
            }

            await _mediator.Send(new RemoveCartItemCommand(userId, dishId));

            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(Guid dishId, int quantity)
        {
            var (userId, userResponse) = await GetCurrentUserAsync();
            if (userResponse == null)
            {
                return RedirectToAction("Authentication", "Account", new { returnUrl = Url.Action(nameof(Cart)) });
            }

            await _mediator.Send(new UpdateCartItemQuantityCommand(userId, dishId, quantity));

            return RedirectToAction(nameof(Cart));
        }

        private async Task<(Guid userId, OperationResult<UserProfile>? userResponse)> GetCurrentUserAsync()
        {
            var userId = await _mediator.Send(new GetCurrentUserIdQuery());
            if (userId == Guid.Empty)
            {
                return (Guid.Empty, null);
            }

            var userResponse = await _mediator.Send(new GetUserProfileByIdQuery(userId));
            return userResponse?.Data?.User == null
                ? (Guid.Empty, null)
                : (userId, userResponse);
        }
    }
}
