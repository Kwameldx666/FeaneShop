using CatalogService.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MenuCatalog>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/catalog", (MenuCatalog catalog) => Results.Ok(catalog.GetMenu()));

app.MapGet("/api/catalog/{id}", (int id, MenuCatalog catalog) =>
{
    var item = catalog.GetMenu().FirstOrDefault(i => i.Id == id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});

app.MapGet("/api/catalog/categories", (MenuCatalog catalog) =>
{
    var categories = catalog.GetMenu()
        .Select(i => i.Category)
        .Distinct()
        .OrderBy(c => c);
    return Results.Ok(categories);
});

app.MapGet("/api/catalog/category/{category}", (string category, MenuCatalog catalog) =>
{
    var items = catalog.GetMenu()
        .Where(i => string.Equals(i.Category, category, StringComparison.OrdinalIgnoreCase));
    return Results.Ok(items);
});

app.Run();
