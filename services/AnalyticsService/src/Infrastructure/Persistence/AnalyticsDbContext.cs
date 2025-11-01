using AnalyticsService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AnalyticsService.Infrastructure.Persistence;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
    {
    }

    public DbSet<OrderStatistics> OrderStatistics { get; set; }
    public DbSet<ProductStatistics> ProductStatistics { get; set; }
    public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // OrderStatistics configuration
        modelBuilder.Entity<OrderStatistics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.AverageOrderValue).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.CreatedAt);
        });

        // ProductStatistics configuration
        modelBuilder.Entity<ProductStatistics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TotalRevenue).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => new { e.ProductId, e.Date });
        });

        // AnalyticsEvent configuration
        modelBuilder.Entity<AnalyticsEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });
    }
}