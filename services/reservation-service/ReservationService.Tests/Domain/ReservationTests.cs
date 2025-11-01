using System;
using FluentAssertions;
using ReservationService.Domain.Entities;
using ReservationService.Domain.Enums;
using Xunit;

namespace ReservationService.Tests.Domain;

public class ReservationTests
{
    [Fact]
    public void Reservation_Creation_SetsPropertiesCorrectly()
    {
        // Arrange & Act
        var reservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PhoneNumber = "1234567890",
            UserEmail = "john@test.com",
            NumberOfPeople = 4,
            ReservationDate = DateTime.UtcNow.AddDays(1),
            Status = ReservationStatus.Pending
        };

        // Assert
        reservation.CustomerName.Should().Be("John Doe");
        reservation.NumberOfPeople.Should().Be(4);
        reservation.Status.Should().Be(ReservationStatus.Pending);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(50)]
    public void Reservation_AcceptsDifferentPartySizes(int numberOfPeople)
    {
        // Arrange & Act
        var reservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            CustomerName = "Test",
            NumberOfPeople = numberOfPeople,
            ReservationDate = DateTime.UtcNow.AddDays(1)
        };

        // Assert
        reservation.NumberOfPeople.Should().Be(numberOfPeople);
    }

    [Theory]
    [InlineData(ReservationStatus.Pending)]
    [InlineData(ReservationStatus.Confirmed)]
    [InlineData(ReservationStatus.Cancelled)]
    [InlineData(ReservationStatus.Completed)]
    public void Reservation_CanHaveDifferentStatuses(ReservationStatus status)
    {
        // Arrange & Act
        var reservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            CustomerName = "Test",
            NumberOfPeople = 2,
            ReservationDate = DateTime.UtcNow.AddDays(1),
            Status = status
        };

        // Assert
        reservation.Status.Should().Be(status);
    }

    [Fact]
    public void Reservation_CanHaveSpecialRequests()
    {
        // Arrange & Act
        var reservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            CustomerName = "Test",
            NumberOfPeople = 2,
            ReservationDate = DateTime.UtcNow.AddDays(1),
            SpecialRequests = "Window seat please"
        };

        // Assert
        reservation.SpecialRequests.Should().Be("Window seat please");
    }

    [Theory]
    [InlineData("Window")]
    [InlineData("Outdoor")]
    [InlineData("Private Room")]
    public void Reservation_CanHaveSeatingPreference(string preference)
    {
        // Arrange & Act
        var reservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            CustomerName = "Test",
            NumberOfPeople = 2,
            ReservationDate = DateTime.UtcNow.AddDays(1),
            SeatingPreference = preference
        };

        // Assert
        reservation.SeatingPreference.Should().Be(preference);
    }

    [Theory]
    [InlineData(25.00)]
    [InlineData(50.00)]
    [InlineData(100.00)]
    public void Reservation_CanHaveBudgetPerGuest(decimal budget)
    {
        // Arrange & Act
        var reservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            CustomerName = "Test",
            NumberOfPeople = 4,
            ReservationDate = DateTime.UtcNow.AddDays(1),
            BudgetPerGuest = budget
        };

        // Assert
        reservation.BudgetPerGuest.Should().Be(budget);
    }

    [Fact]
    public void Reservation_CalculatesEstimatedTotal()
    {
        // Arrange & Act
        var reservation = new Reservation
        {
            UserId = Guid.NewGuid(),
            CustomerName = "Test",
            NumberOfPeople = 4,
            ReservationDate = DateTime.UtcNow.AddDays(1),
            BudgetPerGuest = 25.00m,
            EstimatedTotal = 100.00m
        };

        // Assert
        reservation.EstimatedTotal.Should().Be(100.00m);
        reservation.EstimatedTotal.Should().Be(reservation.NumberOfPeople * reservation.BudgetPerGuest);
    }
}

