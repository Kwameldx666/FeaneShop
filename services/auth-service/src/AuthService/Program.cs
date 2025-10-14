using System.Reflection;
using System.Text;
using AuthService.Application.Configuration;
using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using AuthService.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- JWT настройки ----------
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

if (jwtOptions is null || !jwtOptions.IsValid())
    throw new InvalidOperationException("JWT settings are not configured correctly.");

// ---------- База данных ----------
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Аутентификация ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
        };
    });

// ---------- CORS ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ---------- DI ----------
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ---------- Контроллеры ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ---------- Миграции базы данных ----------
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();
}

// ---------- Middleware ----------
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseJwtCookieAuthentication();

// ---------- Фронтенд (wwwroot) ----------
app.UseDefaultFiles(); // ищет index.html
app.UseStaticFiles();  // отдаёт css/js/html

// ---------- Контроллеры ----------
try
{
    app.MapControllers();
}
catch (ReflectionTypeLoadException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("❌ Ошибка загрузки типов:");
    foreach (var e in ex.LoaderExceptions)
    {
        Console.WriteLine(e?.Message);
    }
    Console.ResetColor();
}
app.Run();