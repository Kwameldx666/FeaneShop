var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient("user-service", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Downstream:user-service"] ?? "http://user-service:5001");
});

builder.Services.AddHttpClient("product-service", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Downstream:product-service"] ?? "http://product-service:5002");
});

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { service = "gateway", status = "healthy" }));

app.Run();
