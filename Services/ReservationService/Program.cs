using Feane.Contracts.Reservations;
using ReservationService.Extensions;
using ReservationService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var group = app.MapGroup("/api/reservations").WithOpenApi();

group.MapPost("/", async (HttpRequest request, CreateReservationRequest reservationRequest, IReservationRepository repository, CancellationToken cancellationToken) =>
{
    if (!TryExtractUserId(request, out var userId))
    {
        return Results.BadRequest(new { message = "Missing or invalid X-User-Id header." });
    }

    var reservation = await repository.CreateAsync(userId, reservationRequest, cancellationToken);
    return Results.Created($"/api/reservations/{reservation.Id}", reservation.ToHistoryItem());
});

group.MapGet("/history/{userId:guid}", async (Guid userId, IReservationRepository repository, CancellationToken cancellationToken) =>
{
    var history = await repository.GetUserHistoryAsync(userId, cancellationToken);
    return Results.Ok(new ReservationHistoryPageModel
    {
        Reservations = history.ToHistoryItems().ToArray(),
        StatusMessage = history.Count == 0 ? "У вас пока нет бронирований." : null
    });
});

app.Run();

static bool TryExtractUserId(HttpRequest request, out Guid userId)
{
    userId = Guid.Empty;
    if (!request.Headers.TryGetValue("X-User-Id", out var headerValues))
    {
        return false;
    }

    var headerValue = headerValues.FirstOrDefault();
    return Guid.TryParse(headerValue, out userId);
}
