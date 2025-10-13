using ReservationService.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ReservationStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/reservations", (ReservationStore store) => Results.Ok(store.GetReservations()));

app.MapGet("/api/reservations/{id}", (Guid id, ReservationStore store) =>
{
    var reservation = store.GetReservations().FirstOrDefault(r => r.Id == id);
    return reservation is not null ? Results.Ok(reservation) : Results.NotFound();
});

app.MapPost("/api/reservations", (CreateReservationRequest request, ReservationStore store) =>
{
    var reservation = store.Create(request);
    return Results.Created($"/api/reservations/{reservation.Id}", reservation);
});

app.MapPost("/api/reservations/{id}/cancel", (Guid id, ReservationStore store) =>
{
    var reservation = store.Cancel(id);
    return reservation is not null ? Results.Ok(reservation) : Results.NotFound();
});

app.Run();
