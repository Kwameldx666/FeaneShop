using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;
using System.Linq;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
if (!jwtSettings.Exists())
{
    throw new InvalidOperationException("JwtSettings section is missing. Provide Issuer, Audience and SecretKey values.");
}

var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured.");
var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JwtSettings:Audience is not configured.");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);
var configuredOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
    .Select(origin => origin?.Trim())
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .ToArray();

var originSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

if (configuredOrigins != null && configuredOrigins.Length > 0)
{
    originSet.UnionWith(configuredOrigins);
}
else
{
    originSet.UnionWith(new[]
    {
        "http://localhost:5003",
        "http://localhost:61370",
        "https://localhost:61369",
        "http://localhost:5000"
    });
}

var allowLocalhostWildcard = builder.Configuration.GetValue("Cors:AllowLocalhostWildcard", true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                return false;
            }

            if (originSet.Contains(origin))
            {
                return true;
            }

            if (allowLocalhostWildcard && Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            {
                return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

await app.UseOcelot();
app.Run();
