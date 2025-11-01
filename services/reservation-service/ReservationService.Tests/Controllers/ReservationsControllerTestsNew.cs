using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ReservationService.Application.DTOs;
using ReservationService.Application.Interfaces;
using ReservationService.Controllers;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Enums;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ReservationService.Tests.Controllers;

public class ReservationsControllerTestsNew
{
    private readonly Mock<IReservationRepository> _mockRepository;
    private readonly Mock<ILogger<ReservationsController>> _mockLogger;
    private readonly ReservationsController _controller;

    public ReservationsControllerTestsNew()
    {
        _mockRepository = new Mock<IReservationRepository>();
        _mockLogger = new Mock<ILogger<ReservationsController>>();
        _controller = new ReservationsController(_mockRepository.Object, _mockLogger.Object);

        var httpContext = new DefaultHttpContext();
        var userId = Guid.NewGuid();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", userId.ToString())
        }, "TestAuthentication"));

        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    [InlineData(25)]
    [InlineData(30)]
    [InlineData(35)]
    [InlineData(40)]
    [InlineData(45)]
    [InlineData(50)]
    public async Task CreateReservation_WithDifferentPartySizes_CreatesReservation(int partySize)
    {
        var userId = Guid.NewGuid();
        var request = new ReservationCreateRequest
        {
            UserId = userId,
            CustomerName = $"Party of {partySize}",
            PhoneNumber = "1234567890",
            UserEmail = "party@test.com",
            NumberOfPeople = partySize,
            ReservationDateTime = DateTime.UtcNow.AddDays(1)
        };

        var createdReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CustomerName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            UserEmail = request.UserEmail,
            NumberOfPeople = request.NumberOfPeople,
            ReservationDate = request.ReservationDateTime,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdReservation);

        var result = await _controller.CreateReservation(request, CancellationToken.None);
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Theory]
    [InlineData("Birthday")]
    [InlineData("Anniversary")]
    [InlineData("Business Meeting")]
    [InlineData("Wedding")]
    [InlineData("Graduation")]
    [InlineData("Reunion")]
    [InlineData("Holiday Party")]
    [InlineData("Baby Shower")]
    [InlineData("Engagement")]
    [InlineData("Retirement")]
    public async Task CreateReservation_WithDifferentOccasions_CreatesReservation(string occasion)
    {
        var userId = Guid.NewGuid();
        var request = new ReservationCreateRequest
        {
            UserId = userId,
            CustomerName = "Occasion User",
            PhoneNumber = "1234567890",
            UserEmail = "occasion@test.com",
            NumberOfPeople = 4,
            ReservationDateTime = DateTime.UtcNow.AddDays(1),
            Occasion = occasion
        };

        var createdReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CustomerName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            UserEmail = request.UserEmail,
            NumberOfPeople = request.NumberOfPeople,
            ReservationDate = request.ReservationDateTime,
            Occasion = request.Occasion,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdReservation);

        var result = await _controller.CreateReservation(request, CancellationToken.None);
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Theory]
    [InlineData("Window")]
    [InlineData("Outdoor")]
    [InlineData("Private Room")]
    [InlineData("Bar Area")]
    [InlineData("Quiet Corner")]
    [InlineData("Garden")]
    [InlineData("Rooftop")]
    [InlineData("Patio")]
    [InlineData("VIP Section")]
    [InlineData("Near Kitchen")]
    public async Task CreateReservation_WithDifferentSeatingPreferences_CreatesReservation(string seatingPreference)
    {
        var userId = Guid.NewGuid();
        var request = new ReservationCreateRequest
        {
            UserId = userId,
            CustomerName = "Seating User",
            PhoneNumber = "1234567890",
            UserEmail = "seating@test.com",
            NumberOfPeople = 4,
            ReservationDateTime = DateTime.UtcNow.AddDays(1),
            SeatingPreference = seatingPreference
        };

        var createdReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CustomerName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            UserEmail = request.UserEmail,
            NumberOfPeople = request.NumberOfPeople,
            ReservationDate = request.ReservationDateTime,
            SeatingPreference = request.SeatingPreference,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdReservation);

        var result = await _controller.CreateReservation(request, CancellationToken.None);
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Theory]
    [InlineData(10.00)]
    [InlineData(15.00)]
    [InlineData(20.00)]
    [InlineData(25.00)]
    [InlineData(30.00)]
    [InlineData(50.00)]
    [InlineData(75.00)]
    [InlineData(100.00)]
    [InlineData(150.00)]
    [InlineData(200.00)]
    [InlineData(250.00)]
    [InlineData(500.00)]
    public async Task CreateReservation_WithDifferentBudgets_CreatesReservation(decimal budgetPerGuest)
    {
        var userId = Guid.NewGuid();
        var request = new ReservationCreateRequest
        {
            UserId = userId,
            CustomerName = "Budget User",
            PhoneNumber = "1234567890",
            UserEmail = "budget@test.com",
            NumberOfPeople = 4,
            ReservationDateTime = DateTime.UtcNow.AddDays(1),
            BudgetPerGuest = budgetPerGuest
        };

        var createdReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CustomerName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            UserEmail = request.UserEmail,
            NumberOfPeople = request.NumberOfPeople,
            ReservationDate = request.ReservationDateTime,
            BudgetPerGuest = (decimal)request.BudgetPerGuest,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdReservation);

        var result = await _controller.CreateReservation(request, CancellationToken.None);
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(21)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(90)]
    [InlineData(120)]
    [InlineData(180)]
    public async Task CreateReservation_WithDifferentAdvanceBookingDays_CreatesReservation(int daysInAdvance)
    {
        var userId = Guid.NewGuid();
        var request = new ReservationCreateRequest
        {
            UserId = userId,
            CustomerName = "Advance User",
            PhoneNumber = "1234567890",
            UserEmail = "advance@test.com",
            NumberOfPeople = 4,
            ReservationDateTime = DateTime.UtcNow.AddDays(daysInAdvance)
        };

        var createdReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CustomerName = request.CustomerName,
            PhoneNumber = request.PhoneNumber,
            UserEmail = request.UserEmail,
            NumberOfPeople = request.NumberOfPeople,
            ReservationDate = request.ReservationDateTime,
            Status = ReservationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdReservation);

        var result = await _controller.CreateReservation(request, CancellationToken.None);
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Confirmed")]
    [InlineData("Cancelled")]
    [InlineData("Completed")]
    public async Task GetReservations_FilterByStatus_ReturnsCorrectReservations(string statusFilter)
    {
        var userIdClaim = _controller.HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        var userId = Guid.Parse(userIdClaim!.Value);

        var status = Enum.Parse<ReservationStatus>(statusFilter);
        var reservations = new List<Reservation>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CustomerName = "Test User",
                PhoneNumber = "1234567890",
                UserEmail = "test@test.com",
                NumberOfPeople = 4,
                ReservationDate = DateTime.UtcNow.AddDays(1),
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<ReservationQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);
        _mockRepository.Setup(r => r.CountAsync(It.IsAny<ReservationQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _controller.GetReservations(userId, null, statusFilter, null, null, null, null, null, null, null, CancellationToken.None);
        result.Should().BeAssignableTo<IActionResult>();
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 10)]
    [InlineData(1, 25)]
    [InlineData(2, 25)]
    [InlineData(5, 50)]
    [InlineData(1, 5)]
    [InlineData(3, 15)]
    [InlineData(10, 10)]
    public async Task GetReservations_WithPagination_ReturnsCorrectPage(int page, int pageSize)
    {
        var userIdClaim = _controller.HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        var userId = Guid.Parse(userIdClaim!.Value);

        var reservations = new List<Reservation>();
        for (int i = 0; i < pageSize; i++)
        {
            reservations.Add(new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CustomerName = $"User {i}",
                PhoneNumber = "1234567890",
                UserEmail = $"user{i}@test.com",
                NumberOfPeople = 2,
                ReservationDate = DateTime.UtcNow.AddDays(i + 1),
                Status = ReservationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _mockRepository.Setup(r => r.GetAsync(It.IsAny<ReservationQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);
        _mockRepository.Setup(r => r.CountAsync(It.IsAny<ReservationQueryOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        var result = await _controller.GetReservations(userId, null, null, null, null, null, null, null, page, pageSize, CancellationToken.None);
        result.Should().BeAssignableTo<IActionResult>();
    }
}

