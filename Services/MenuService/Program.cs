using Feane.Contracts.Dishes;
using MenuService.Extensions;
using MenuService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDishRepository, InMemoryDishRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var group = app.MapGroup("/api/dishes").WithOpenApi();

group.MapGet("/", async (IDishRepository repository, CancellationToken cancellationToken) =>
{
    var dishes = await repository.GetAllAsync(cancellationToken);
    return Results.Ok(dishes.ToResponseCollection());
});

group.MapGet("/{id:guid}", async (Guid id, IDishRepository repository, CancellationToken cancellationToken) =>
{
    var dish = await repository.GetByIdAsync(id, cancellationToken);
    return dish is null ? Results.NotFound() : Results.Ok(dish.ToResponse());
});

group.MapPost("/", async (CreateDishRequest request, IDishRepository repository, CancellationToken cancellationToken) =>
{
    var dish = await repository.CreateAsync(request, cancellationToken);
    return Results.Created($"/api/dishes/{dish.Id}", dish.ToResponse());
});

group.MapPut("/{id:guid}", async (Guid id, UpdateDishRequest request, IDishRepository repository, CancellationToken cancellationToken) =>
{
    var dish = await repository.UpdateAsync(id, request, cancellationToken);
    return dish is null ? Results.NotFound() : Results.Ok(dish.ToResponse());
});

group.MapDelete("/{id:guid}", async (Guid id, IDishRepository repository, CancellationToken cancellationToken) =>
{
    var removed = await repository.DeleteAsync(id, cancellationToken);
    return removed ? Results.NoContent() : Results.NotFound();
});

group.MapPost("/seed/{count:int}", async (int count, IDishRepository repository, CancellationToken cancellationToken) =>
{
    if (count <= 0 || count > 100)
    {
        return Results.BadRequest(new { message = "Count must be between 1 and 100." });
    }

    var result = await repository.SeedAsync(count, cancellationToken);
    return Results.Ok(new { result.created, result.skipped });
});

app.Run();
