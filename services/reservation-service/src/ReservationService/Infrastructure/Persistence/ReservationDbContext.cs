using Microsoft.EntityFrameworkCore;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Enums;

namespace ReservationService.Infrastructure.Persistence;

public class ReservationDbContext : DbContext
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options) : base(options)
    {
    }

    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var entity = modelBuilder.Entity<Reservation>();
        entity.ToTable("Reservations");
        entity.HasKey(r => r.Id);
        entity.Property(r => r.CustomerName).IsRequired().HasMaxLength(100);
        entity.Property(r => r.PhoneNumber).IsRequired().HasMaxLength(32);
        entity.Property(r => r.UserEmail).IsRequired().HasMaxLength(128);
        entity.Property(r => r.NumberOfPeople).IsRequired();
        entity.Property(r => r.ReservationDate).IsRequired();
        entity.Property(r => r.Occasion).HasMaxLength(64);
        entity.Property(r => r.SeatingPreference).HasMaxLength(64);
        entity.Property(r => r.SpecialRequests).HasMaxLength(1024);
        entity.Property(r => r.BudgetPerGuest).HasPrecision(10, 2);
        entity.Property(r => r.EstimatedTotal).HasPrecision(12, 2);
        entity.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(ReservationStatus.Pending);
        entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        entity.Property(r => r.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        entity.Property(r => r.CancelledAt);
        entity.HasIndex(r => r.UserEmail);
        entity.HasIndex(r => r.ReservationDate);
        entity.HasIndex(r => r.Status);

        var now = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var seed = new List<Reservation>
        {
            new()
            {
                Id = Guid.Parse("2C792C55-79F2-4D20-A5B9-56664FEE5B8B"),
                CustomerName = "Мария Иванова",
                PhoneNumber = "+37360000001",
                UserEmail = "maria@example.com",
                NumberOfPeople = 2,
                ReservationDate = now.AddDays(2),
                Occasion = "romantic",
                SeatingPreference = "window",
                SpecialRequests = "Свечи и цветы",
                BudgetPerGuest = 25.5m,
                EstimatedTotal = 51m,
                Status = ReservationStatus.Confirmed,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                Id = Guid.Parse("C7B75C6B-ADBD-4872-83F9-82E927CBF97F"),
                CustomerName = "Алексей Петров",
                PhoneNumber = "+37360000002",
                UserEmail = "alexey@example.com",
                NumberOfPeople = 6,
                ReservationDate = now.AddDays(-5),
                Occasion = "birthday",
                SeatingPreference = "indoor",
                SpecialRequests = "Торт и шарики",
                BudgetPerGuest = 18m,
                EstimatedTotal = 108m,
                Status = ReservationStatus.Completed,
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now.AddDays(-4)
            },
            new()
            {
                Id = Guid.Parse("0B24B232-5D9E-4E8E-91B1-94EA0C6C0F4A"),
                CustomerName = "Елена Сидорова",
                PhoneNumber = "+37360000003",
                UserEmail = "elena@example.com",
                NumberOfPeople = 4,
                ReservationDate = now.AddDays(7),
                Occasion = "business",
                SeatingPreference = "quiet",
                SpecialRequests = "Проектор",
                BudgetPerGuest = 32m,
                EstimatedTotal = 128m,
                Status = ReservationStatus.Pending,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now.AddDays(-1)
            }
        };

        entity.HasData(seed);
    }
}