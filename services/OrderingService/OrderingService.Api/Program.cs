using OrderingService.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<OrderStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/orders", (OrderStore store) => Results.Ok(store.GetOrders()));

app.MapGet("/api/orders/{id}", (Guid id, OrderStore store) =>
{
    var order = store.GetOrders().FirstOrDefault(o => o.Id == id);
    return order is not null ? Results.Ok(order) : Results.NotFound();
});

app.MapPost("/api/orders", (CreateOrderRequest request, OrderStore store) =>
{
    var order = store.CreateOrder(request);
    return Results.Created($"/api/orders/{order.Id}", order);
});

app.MapPost("/api/orders/{id}/status", (Guid id, UpdateOrderStatusRequest request, OrderStore store) =>
{
    var order = store.UpdateStatus(id, request.Status);
    return order is not null ? Results.Ok(order) : Results.NotFound();
});

app.Run();
