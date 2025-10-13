using Microsoft.AspNetCore.Mvc;
using PaymentService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IPaymentProcessor, FakePaymentProcessor>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "payment-service", status = "healthy" }));

app.Run();

namespace PaymentService.Models;

public interface IPaymentProcessor
{
    PaymentReceipt Authorize(PaymentRequest request);
}

public sealed class FakePaymentProcessor : IPaymentProcessor
{
    public PaymentReceipt Authorize(PaymentRequest request)
    {
        var status = request.Amount > 0 ? PaymentStatus.Authorized : PaymentStatus.Declined;
        return new PaymentReceipt(Guid.NewGuid(), status, DateTimeOffset.UtcNow, request.Amount, request.Currency);
    }
}

public record PaymentRequest(Guid OrderId, decimal Amount, string Currency, string ProviderReference);

public record PaymentReceipt(Guid PaymentId, PaymentStatus Status, DateTimeOffset ProcessedAt, decimal Amount, string Currency);

public enum PaymentStatus
{
    Authorized,
    Declined
}
