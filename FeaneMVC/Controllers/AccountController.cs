using FeaneMVC.Application.Configuration;
using FeaneMVC.Application.Commands.Reservations;
using FeaneMVC.Application.Commands.Sessions;
using FeaneMVC.Application.Commands.Users;
using FeaneMVC.Application.Queries.Authentication;
using FeaneMVC.Application.Queries.Reservations;
using FeaneMVC.Application.Queries.Sessions;
using FeaneMVC.Application.Queries.Users;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Enums;
using FeaneMVC.Contracts.Account;
using FeaneMVC.Contracts.Reservations;
using FeaneMVC.Extenstions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace FeaneMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IMediator _mediator;
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<AccountController> _logger;
        private readonly SignInManager<UserData> _signInManager;
        private readonly UserManager<UserData> _userManager;
        private readonly EmailAddressAttribute _emailValidator = new();

        // Constructor to initialize dependencies
        public AccountController(
            IMediator mediator,
            IOptions<JwtOptions> jwtOptions,
            ILogger<AccountController> logger,
            SignInManager<UserData> signInManager,
            UserManager<UserData> userManager)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _jwtOptions = jwtOptions.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            if (!_jwtOptions.IsValid())
            {
                throw new InvalidOperationException("JWT settings are not configured correctly.");
            }
        }

        // GET: /Account/Authentication
        [AllowAnonymous]
        public IActionResult Authentication(string? returnUrl = null, string? authMode = null)
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogDebug("Authenticated user requested Authentication page; redirecting to profile or returnUrl.");
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    var decodedReturnUrl = Uri.UnescapeDataString(returnUrl);
                    if (Url.IsLocalUrl(decodedReturnUrl))
                    {
                        return Redirect(decodedReturnUrl);
                    }
                }

                return RedirectToAction(nameof(Profile));
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["AuthMode"] = string.Equals(authMode, "register", StringComparison.OrdinalIgnoreCase)
                ? "register"
                : null;

            return View();
        }

        // GET: /Account/Profile
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Profile()
        {
            Guid userId = await _mediator.Send(new GetCurrentUserIdQuery());
            if (userId == Guid.Empty)
            {
                _logger.LogInformation("Profile requested by anonymous user. Redirecting to Authentication.");
                return RedirectToAction(nameof(Authentication), new { returnUrl = Url.Action(nameof(Profile)) });
            }

            var user = await _mediator.Send(new GetUserProfileByIdQuery(userId));

            if (user?.Status == true && user.Data?.User != null)
            {
                var reservations = await _mediator.Send(new GetReservationsByUserIdQuery(userId));
                var pageModel = new ReservationHistoryPageModel
                {
                    Reservations = reservations.ToHistoryItems(),
                    StatusMessage = TempData.TryGetValue("ReservationStatusMessage", out var status) ? status as string : null,
                    ErrorMessage = TempData.TryGetValue("ReservationErrorMessage", out var error) ? error as string : null
                };

                TempData.Remove("ReservationStatusMessage");
                TempData.Remove("ReservationErrorMessage");

                return View(pageModel);
            }

            _logger.LogWarning("Profile information for user {UserId} is unavailable; redirecting to Authentication.", userId);
            return RedirectToAction(nameof(Authentication), new { returnUrl = Url.Action(nameof(Profile)) });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(Guid id)
        {
            var userId = await _mediator.Send(new GetCurrentUserIdQuery());

            if (userId == Guid.Empty)
            {
                _logger.LogInformation("CancelReservation attempted by anonymous user. Redirecting to Authentication.");
                return RedirectToAction(nameof(Authentication), new { returnUrl = Url.Action(nameof(Profile)) });
            }

            var result = await _mediator.Send(new CancelReservationCommand(id, userId));

            if (result.Status)
            {
                TempData["ReservationStatusMessage"] = string.IsNullOrWhiteSpace(result.Message)
                    ? "Резервация успешно отменена."
                    : result.Message;
            }
            else
            {
                TempData["ReservationErrorMessage"] = result.Message ?? "Не удалось отменить резервацию.";
            }

            return RedirectToAction(nameof(Profile));
        }

        // GET: /Account/Contacts
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Contacts()
        {
            Guid userId = await _mediator.Send(new GetCurrentUserIdQuery());

            if (userId == Guid.Empty)
            {
                _logger.LogInformation("Contacts requested by anonymous user. Redirecting to Authentication.");
                return RedirectToAction(nameof(Authentication), new { returnUrl = Url.Action(nameof(Contacts)) });
            }

            var user = await _mediator.Send(new GetUserProfileByIdQuery(userId));

            if (user.Status && user.Data?.User != null)
            {
                return View(user.Data.User);
            }

            _logger.LogWarning("Failed to load contacts for user {UserId}. Redirecting to Authentication.", userId);
            return RedirectToAction(nameof(Authentication), new { returnUrl = Url.Action(nameof(Contacts)) });
        }

        // GET: /Account/Discounts
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Discounts()
        {
            return View();
        }

        // GET: /Account/Addresses
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Addresses()
        {
            Guid userId = await _mediator.Send(new GetCurrentUserIdQuery());

            if (userId == Guid.Empty)
            {
                _logger.LogInformation("Addresses requested by anonymous user. Redirecting to Authentication.");
                return RedirectToAction(nameof(Authentication), new { returnUrl = Url.Action(nameof(Addresses)) });
            }

            var addressResponse = await _mediator.Send(new GetUserAddressQuery(userId));

            if (addressResponse.Status)
            {
                return View(addressResponse.Data);
            }

            _logger.LogWarning("Failed to load addresses for user {UserId}. Redirecting to Authentication.", userId);
            return RedirectToAction(nameof(Authentication), new { returnUrl = Url.Action(nameof(Addresses)) });
        }

        // POST: /Account/UpdateContacts
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<JsonResult> UpdateContacts(UserData data)
        {
            // Retrieve the user ID from the session or another method
            Guid userId = await _mediator.Send(new GetCurrentUserIdQuery());

            if (userId == Guid.Empty)
            {
                _logger.LogInformation("UpdateContacts attempted by anonymous user.");
                return Json(new { success = false, message = "User is not authenticated." });
            }

            // Assign the valid user ID to the data object
            data.Id = userId;

            // Attempt to update the user information
            var updateResponse = await _mediator.Send(new UpdateUserCommand(data));

            // If the update operation fails, return a JSON response with the error message
            if (!updateResponse.Status)
            {
                return Json(new { success = false, message = updateResponse.Message });
            }

            // If the update operation succeeds, return a JSON response with success message and updated user data
            return Json(new { success = true, message = "User updated successfully.", user = updateResponse.Data?.User });
        }

        // POST: /Account/UpdateAddress
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<JsonResult> UpdateAddress(DeliveryAddress data)
        {
            Guid userId = await _mediator.Send(new GetCurrentUserIdQuery());

            if (userId == Guid.Empty)
            {
                _logger.LogInformation("UpdateAddress attempted by anonymous user.");
                return Json(new { success = false, message = "User is not authenticated." });
            }

            var userResponse = await _mediator.Send(new GetUserProfileByIdQuery(userId));

            if (!userResponse.Status || userResponse.Data?.User == null)
            {
                _logger.LogWarning("Failed to load user profile for address update for user {UserId}: {Message}", userId, userResponse.Message);
                return Json(new { success = false, message = userResponse.Message ?? "Unable to load user profile." });
            }

            var updateResponse = await _mediator.Send(new UpdateUserAddressCommand(userResponse.Data.User, data));

            if (!updateResponse.Status)
            {
                _logger.LogWarning("Failed to update address for user {UserId}: {Message}", userId, updateResponse.Message);
                return Json(new { success = false, message = updateResponse.Message });
            }

            return Json(new { success = true, message = "User updated successfully.", user = updateResponse.Data?.User });
        }

        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return RedirectToAction("ResetPassword");
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword()
        {
            return View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Error = "Please enter a valid email address.";
                return View();
            }

            var resetResponse = await _mediator.Send(new ChangeUserPasswordCommand(email));

            if (resetResponse.Status)
            {
                // Logic for sending the new password to the user's email
                ViewBag.Message = $"A new password has been sent to {email}.";
            }
            else
            {
                ViewBag.Error = resetResponse.Message ?? "Failed to reset password. Please try again.";
            }

            return RedirectToAction("Authentication");
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(AuthenticationRequest data)
        {
            if (data == null)
            {
                return RedirectToAction("Error404", "Error");
            }

            var returnUrl = data.ReturnUrl;

            var registerData = new UserData
            {
                Id = Guid.NewGuid(),
                Email = data.Email?.Trim() ?? string.Empty,
                Username = data.Username?.Trim() ?? string.Empty,
                Roles = Role.User,
                IP = HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                FirstRegisterTime = DateTime.UtcNow,
                IsActive = true,
                Credential = data.Email?.Trim() ?? data.Username?.Trim(),
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                LockoutEnabled = false,
                TwoFactorEnabled = false
            };

            var identityResult = await _userManager.CreateAsync(registerData, data.Password);

            if (!identityResult.Succeeded)
            {
                var errorMessage = string.Join(" ", identityResult.Errors.Select(e => e.Description).Where(description => !string.IsNullOrWhiteSpace(description)));
                TempData["RegisterError"] = string.IsNullOrWhiteSpace(errorMessage) ? "Registration failed." : errorMessage;
                _logger.LogWarning("Registration failed for email {Email}: {Errors}", registerData.Email, errorMessage);
                return RedirectToAction("Authentication", new { returnUrl, authMode = "register" });
            }

            TempData["RegisterSuccess"] = "Registration successful. Please log in.";
            _logger.LogInformation("User {UserId} registered successfully via SignInManager integration.", registerData.Id);
            return RedirectToAction("Authentication", new { returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(AuthenticationRequest data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Credential) || string.IsNullOrWhiteSpace(data.Password))
            {
                TempData["ErrorMessage"] = "Please enter both credential and password.";
                _logger.LogWarning("Login attempt with missing credentials.");
                return RedirectToAction("Authentication", new { returnUrl = data?.ReturnUrl });
            }

            var credential = data.Credential.Trim();
            UserData? user = null;

            if (_emailValidator.IsValid(credential))
            {
                user = await _userManager.FindByEmailAsync(credential);
            }

            if (user == null)
            {
                user = await _userManager.FindByNameAsync(credential);
            }

            if (user == null)
            {
                TempData["ErrorMessage"] = "Invalid credentials.";
                _logger.LogWarning("Authentication failed. No user found for credential {Credential}.", credential);
                return RedirectToAction("Authentication", new { returnUrl = data.ReturnUrl });
            }

            var signInResult = await _signInManager.PasswordSignInAsync(user, data.Password, data.RememberMe, lockoutOnFailure: false);

            if (!signInResult.Succeeded)
            {
                if (signInResult.IsLockedOut)
                {
                    TempData["ErrorMessage"] = "Account temporarily locked. Please try again later.";
                }
                else if (signInResult.RequiresTwoFactor)
                {
                    TempData["ErrorMessage"] = "Two-factor authentication is required.";
                }
                else if (signInResult.IsNotAllowed)
                {
                    TempData["ErrorMessage"] = "Authentication is not allowed for this account.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Invalid credentials.";
                }

                _logger.LogWarning("Sign-in failed for user {UserId}: {Reason}", user.Id, signInResult.ToString());
                return RedirectToAction("Authentication", new { returnUrl = data.ReturnUrl });
            }

            if (user.FirstLoginTime == default)
            {
                user.FirstLoginTime = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
            }

            var cookieValue = await _mediator.Send(new SetUserCookieCommand(user.Id, credential, data.RememberMe));
            await _mediator.Send(new SetSessionValueCommand("IsUserLoggedIn", "true"));
            await _mediator.Send(new SetSessionValueCommand("UserId", user.Id.ToString()));
            await _mediator.Send(new SetSessionValueCommand("UserRole", user.Roles.ToString()));
            ViewBag.UserName = user.Username;

            var token = await _mediator.Send(new GenerateJwtTokenQuery(user));
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = HttpContext.Request.IsHttps,
                IsEssential = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes)
            };
            Response.Cookies.Append(_jwtOptions.CookieName, token, cookieOptions);

            if (!string.IsNullOrWhiteSpace(data.ReturnUrl))
            {
                var decodedReturnUrl = Uri.UnescapeDataString(data.ReturnUrl);
                if (Url.IsLocalUrl(decodedReturnUrl))
                {
                    _logger.LogInformation("User {UserId} authenticated successfully. Redirecting to returnUrl {ReturnUrl}.", user.Id, decodedReturnUrl);
                    return Redirect(decodedReturnUrl);
                }
            }

            _logger.LogInformation(
                "User {UserId} authenticated successfully with cookie {CookieValue}. Redirecting to profile.",
                user.Id,
                cookieValue);
            return RedirectToAction("Profile");
        }

        // GET: /Account/Logout
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            await _mediator.Send(new UserLogoutCommand());
            Response.Cookies.Delete(_jwtOptions.CookieName);
            _logger.LogInformation("User logged out successfully.");
            return RedirectToAction("Index", "Home");
        }
    }
}
