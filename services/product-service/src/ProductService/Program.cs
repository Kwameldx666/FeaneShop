using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Persistence;
using ProductService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDishRepository, DishRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ProductService.Database");

    try
    {
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Count > 0)
        {
            logger.LogInformation("Applying {MigrationCount} pending migrations: {Migrations}", pendingMigrations.Count, string.Join(", ", pendingMigrations));
            dbContext.Database.Migrate();
        }
        else
        {
            var databaseCreator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
            if (databaseCreator != null && !databaseCreator.HasTables())
            {
                logger.LogWarning("Database has no tables. Creating tables based on the current model.");
                databaseCreator.CreateTables();
            }
            else
            {
                logger.LogInformation("Database schema is up to date. No migrations needed.");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while ensuring the ProductService database is ready.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();

app.Run();
