using FeaneMVC.Application.Commands.Payments;
using FeaneMVC.Application.Queries.Sessions;
using FeaneMVC.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeaneMVC.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class PaymentController : Controller
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    public IActionResult Checkout(decimal amount)
    {
        if (amount <= 0)
        {
            TempData["CartError"] = "Your cart is empty.";
            return RedirectToAction("Cart", "Cart");
        }

        var paymentDetails = new PaymentDetails
        {
            Amount = amount,
            TotalPrice = amount,
            ExpiryDate = DateTime.UtcNow.AddMonths(1),
            Currency = "USD"
        };

        return View(paymentDetails);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitPayment(PaymentDetails paymentDetails)
    {
        if (paymentDetails == null)
        {
            ModelState.AddModelError(string.Empty, "Payment details are required.");
            return View("Checkout", new PaymentDetails());
        }

        if (paymentDetails.TotalPrice <= 0 && paymentDetails.Amount > 0)
        {
            paymentDetails.TotalPrice = paymentDetails.Amount;
        }

        if (paymentDetails.Amount <= 0 && paymentDetails.TotalPrice > 0)
        {
            paymentDetails.Amount = paymentDetails.TotalPrice;
        }

        if (paymentDetails.Amount <= 0 || paymentDetails.TotalPrice <= 0)
        {
            ModelState.AddModelError(nameof(paymentDetails.TotalPrice), "The payment amount must be greater than zero.");
        }

        paymentDetails.Currency = string.IsNullOrWhiteSpace(paymentDetails.Currency)
            ? "USD"
            : paymentDetails.Currency.Trim().ToUpperInvariant();

        if (!ModelState.IsValid)
        {
            return View("Checkout", paymentDetails);
        }

        Guid userId = await _mediator.Send(new GetCurrentUserIdQuery());

        if (userId == Guid.Empty)
        {
            TempData["ErrorMessage"] = "Please log in to complete your purchase.";
            var returnUrl = Url.Action(nameof(Checkout), new { amount = paymentDetails.TotalPrice > 0 ? paymentDetails.TotalPrice : paymentDetails.Amount });
            return RedirectToAction("Authentication", "Account", new { returnUrl });
        }

        var paymentResponse = await _mediator.Send(new ProcessPaymentCommand(userId, paymentDetails));

        if (paymentResponse == null)
        {
            ModelState.AddModelError(string.Empty, "Payment service is temporarily unavailable. Please try again.");
            return View("Checkout", paymentDetails);
        }

        if (paymentResponse.Status)
        {
            TempData["PaymentSuccess"] = paymentResponse.Message ?? "Payment processed successfully.";
            return RedirectToAction("Confirmation");
        }

        ModelState.AddModelError(string.Empty, paymentResponse.Message ?? "Payment processing failed. Please try again.");
        return View("Checkout", paymentDetails);
    }

    public IActionResult Confirmation() => View();
}
