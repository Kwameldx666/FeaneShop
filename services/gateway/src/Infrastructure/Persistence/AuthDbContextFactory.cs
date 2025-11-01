using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthService.Infrastructure.Persistence;

public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, false)
            .AddJsonFile($"appsettings.{environment}.json", true, false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' not found.");

        var builder = new DbContextOptionsBuilder<AuthDbContext>();
        builder.UseSqlServer(connectionString);

        return new AuthDbContext(builder.Options);
    }
}