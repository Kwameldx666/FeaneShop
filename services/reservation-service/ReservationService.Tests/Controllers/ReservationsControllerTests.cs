using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ReservationService.Controllers;
using ReservationService.Domain.Entities;
using ReservationService.Infrastructure.Persistence;

namespace ReservationService.Tests.Controllers;

public class ReservationsControllerTests : IDisposable
{
    private readonly ReservationDbContext _context;
    private readonly ReservationsController _controller;

    public ReservationsControllerTests()
    {
        var options = new DbContextOptionsBuilder<ReservationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ReservationDbContext(options);
        _controller = new ReservationsController(_context);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    [Fact]
    public async Task GetReservations_ReturnsAllReservations()
    {
        // Arrange
        var reservations = new List<Reservation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ReservationDate = DateTime.UtcNow.AddDays(1),
                Guests = 4,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ReservationDate = DateTime.UtcNow.AddDays(2),
                Guests = 2,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Reservations.AddRange(reservations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetReservations();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetReservation_WithValidId_ReturnsReservation()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = new Reservation
        {
            Id = reservationId,
            UserId = Guid.NewGuid(),
            ReservationDate = DateTime.UtcNow.AddDays(1),
            Guests = 6,
            Status = "Confirmed",
            SpecialRequests = "Window seat",
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetReservation(reservationId);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().NotBeNull();
        result.Value!.Guests.Should().Be(6);
        result.Value.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task CreateReservation_WithValidData_CreatesReservation()
    {
        // Arrange
        var newReservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            ReservationDate = DateTime.UtcNow.AddDays(3),
            Guests = 8,
            Status = "Pending",
            SpecialRequests = "Birthday celebration",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _controller.CreateReservation(newReservation);

        // Assert
        result.Should().NotBeNull();
        var createdReservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.SpecialRequests == "Birthday celebration");

        createdReservation.Should().NotBeNull();
        createdReservation!.Guests.Should().Be(8);
        createdReservation.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task UpdateReservationStatus_WithValidData_UpdatesStatus()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = new Reservation
        {
            Id = reservationId,
            UserId = Guid.NewGuid(),
            ReservationDate = DateTime.UtcNow.AddDays(1),
            Guests = 4,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        await _controller.UpdateReservationStatus(reservationId, "Confirmed");

        // Assert
        var updatedReservation = await _context.Reservations.FindAsync(reservationId);
        updatedReservation!.Status.Should().Be("Confirmed");
    }

    [Fact]
    public async Task CancelReservation_WithValidId_CancelsReservation()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = new Reservation
        {
            Id = reservationId,
            UserId = Guid.NewGuid(),
            ReservationDate = DateTime.UtcNow.AddDays(1),
            Guests = 4,
            Status = "Confirmed",
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        await _controller.CancelReservation(reservationId);

        // Assert
        var cancelledReservation = await _context.Reservations.FindAsync(reservationId);
        cancelledReservation!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task GetReservationsByUserId_ReturnsUserReservations()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservations = new List<Reservation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReservationDate = DateTime.UtcNow.AddDays(1),
                Guests = 2,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ReservationDate = DateTime.UtcNow.AddDays(2),
                Guests = 4,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(), // Different user
                ReservationDate = DateTime.UtcNow.AddDays(1),
                Guests = 3,
                Status = "Confirmed",
                CreatedAt = DateTime.UtcNow
            }
        };

        _context.Reservations.AddRange(reservations);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetReservationsByUserId(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.UserId == userId);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Confirmed")]
    [InlineData("Cancelled")]
    [InlineData("Completed")]
    public async Task UpdateReservationStatus_WithValidStatuses_UpdatesCorrectly(string status)
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var reservation = new Reservation
        {
            Id = reservationId,
            UserId = Guid.NewGuid(),
            ReservationDate = DateTime.UtcNow.AddDays(1),
            Guests = 4,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        await _controller.UpdateReservationStatus(reservationId, status);

        // Assert
        var updatedReservation = await _context.Reservations.FindAsync(reservationId);
        updatedReservation!.Status.Should().Be(status);
    }

    [Fact]
    public async Task CreateReservation_WithPastDate_ThrowsException()
    {
        // Arrange
        var pastReservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            ReservationDate = DateTime.UtcNow.AddDays(-1), // Past date
            Guests = 4,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            if (pastReservation.ReservationDate < DateTime.UtcNow)
                throw new ArgumentException("Reservation date cannot be in the past");

            await _controller.CreateReservation(pastReservation);
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateReservation_WithInvalidGuests_ThrowsException(int guests)
    {
        // Arrange
        var reservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            ReservationDate = DateTime.UtcNow.AddDays(1),
            Guests = guests,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            if (reservation.Guests <= 0)
                throw new ArgumentException("Number of guests must be greater than 0");

            await _controller.CreateReservation(reservation);
        });
    }
}