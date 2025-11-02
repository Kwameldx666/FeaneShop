using System.Text;
using AuthService.Application.Clients;
using AuthService.Application.Configuration;
using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Services;
using AuthService.Middleware;
using FeaneGateway.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", false, true);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("JWT settings are not configured.");

if (!jwtOptions.IsValid()) throw new InvalidOperationException("JWT settings are not configured correctly.");

var userServiceBaseUrl = builder.Configuration.GetValue<string>("UserService:BaseUrl") ?? "http://localhost:5020";
if (!Uri.TryCreate(userServiceBaseUrl, UriKind.Absolute, out var userServiceUri))
    throw new InvalidOperationException("UserService:BaseUrl is not a valid absolute URI.");

builder.Services.AddHttpClient<IUserProfileClient, UserProfileClient>(client =>
{
    client.BaseAddress = new Uri(userServiceUri.ToString().TrimEnd('/') + "/");
});

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
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

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal
                    ?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                Console.WriteLine($"JWT Token validated for user: {userId}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);

// Build a clean, non-null list of allowed origins
var configuredOriginsRaw = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
var configuredOrigins = configuredOriginsRaw
    .Select(origin => origin?.Trim())
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Select(origin => origin!)
    .ToArray();

var originSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

if (configuredOrigins.Length > 0)
    originSet.UnionWith(configuredOrigins);
else
    originSet.UnionWith(new[]
    {
        "http://localhost:5003",
        "http://localhost:61370",
        "https://localhost:61369",
        "http://localhost:5000"
    });

var allowLocalhostWildcard = builder.Configuration.GetValue("Cors:AllowLocalhostWildcard", true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrWhiteSpace(origin)) return false;

                if (originSet.Contains(origin)) return true;

                if (allowLocalhostWildcard && Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                    return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                           || string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);

                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    dbContext.Database.Migrate();
}

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseJwtCookieAuthentication();
app.UseAuthentication();
app.UseAuthorization();

// Use top-level route registrations as per analyzer recommendation
app.MapControllers();

await app.UseOcelot();

app.Run();
