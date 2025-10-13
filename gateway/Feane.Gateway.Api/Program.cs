using System.Net.Mime;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient("catalog", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("Downstream:Catalog") ?? "http://catalog-api");
});

builder.Services.AddHttpClient("ordering", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("Downstream:Ordering") ?? "http://ordering-api");
});

builder.Services.AddHttpClient("reservation", client =>
{
    client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("Downstream:Reservation") ?? "http://reservation-api");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/menu", async (IHttpClientFactory httpClientFactory, CancellationToken token) =>
{
    var client = httpClientFactory.CreateClient("catalog");
    var response = await client.GetAsync("/api/catalog", token);
    response.EnsureSuccessStatusCode();
    var stream = await response.Content.ReadAsStreamAsync(token);
    return Results.Stream(stream, contentType: MediaTypeNames.Application.Json);
});

app.MapGet("/api/menu/{category}", async (string category, IHttpClientFactory httpClientFactory, CancellationToken token) =>
{
    var client = httpClientFactory.CreateClient("catalog");
    var response = await client.GetAsync($"/api/catalog/category/{category}", token);
    response.EnsureSuccessStatusCode();
    var stream = await response.Content.ReadAsStreamAsync(token);
    return Results.Stream(stream, contentType: MediaTypeNames.Application.Json);
});

app.MapPost("/api/orders", async (HttpContext context, IHttpClientFactory httpClientFactory, CancellationToken token) =>
{
    var client = httpClientFactory.CreateClient("ordering");
    using var content = new StreamContent(context.Request.Body);
    content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? MediaTypeNames.Application.Json);
    using var response = await client.PostAsync("/api/orders", content, token);
    response.EnsureSuccessStatusCode();
    var payload = await response.Content.ReadAsStringAsync(token);
    return Results.Content(payload, MediaTypeNames.Application.Json);
});

app.MapGet("/api/orders/{id}", async (Guid id, IHttpClientFactory httpClientFactory, CancellationToken token) =>
{
    var client = httpClientFactory.CreateClient("ordering");
    var response = await client.GetAsync($"/api/orders/{id}", token);
    response.EnsureSuccessStatusCode();
    var stream = await response.Content.ReadAsStreamAsync(token);
    return Results.Stream(stream, contentType: MediaTypeNames.Application.Json);
});

app.MapPost("/api/reservations", async (HttpContext context, IHttpClientFactory httpClientFactory, CancellationToken token) =>
{
    var client = httpClientFactory.CreateClient("reservation");
    using var content = new StreamContent(context.Request.Body);
    content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? MediaTypeNames.Application.Json);
    using var response = await client.PostAsync("/api/reservations", content, token);
    response.EnsureSuccessStatusCode();
    var payload = await response.Content.ReadAsStringAsync(token);
    return Results.Content(payload, MediaTypeNames.Application.Json);
});

app.MapGet("/api/reservations/{id}", async (Guid id, IHttpClientFactory httpClientFactory, CancellationToken token) =>
{
    var client = httpClientFactory.CreateClient("reservation");
    var response = await client.GetAsync($"/api/reservations/{id}", token);
    response.EnsureSuccessStatusCode();
    var stream = await response.Content.ReadAsStreamAsync(token);
    return Results.Stream(stream, contentType: MediaTypeNames.Application.Json);
});

app.Run();
