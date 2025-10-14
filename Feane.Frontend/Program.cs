var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");

// Это всё, что нужно фронту:
app.UseDefaultFiles();  // ищет index.html
app.UseStaticFiles();   // отдаёт css/js/html
app.MapFallbackToFile("index.html"); // для SPA

app.Run();
