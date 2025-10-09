using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Domain.Enums;
using FeaneMVC.Domain.ValueObjects;
using FeaneMVC.Infrastructure.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace FeaneMVC.Infrastructure.Persistence.Repositories
{
    public class ReservationRepository : IReservation
    {
        private readonly ApplicationDbContext _context;
        private readonly INotification _notification;

        public ReservationRepository(ApplicationDbContext context, INotification notification)
        {
            _context = context;
            _notification = notification;
        }

        public OperationResult<Reservation> CreateReservation(Reservation reservation, Guid userId)
        {
            if (reservation == null)
            {
                return OperationResult<Reservation>.Failure("Reservation data is null");
            }

            try
            {
                var conflictingReservation = _context.Reservations
                    .FirstOrDefault(r => r.ReservationDate.Date == reservation.ReservationDate.Date &&
                                           r.ReservationDate.TimeOfDay == reservation.ReservationDate.TimeOfDay);

                if (conflictingReservation != null)
                {
                    return OperationResult<Reservation>.Failure("Conflict with an existing reservation at this time.");
                }

                reservation.Status = ReservationStatus.Confirmed;
                reservation.UserId = userId;

                var reservationHistory = new ReservationHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ReservationDate = reservation.ReservationDate,
                    Status = reservation.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ReservationsHistory.Add(reservationHistory);
                _context.Reservations.Add(reservation);
                _context.SaveChanges();

                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    var message = $"Dear {reservation.CustomerName}, your reservation for {reservation.NumberOfPeople} people on {reservation.ReservationDate} has been created successfully.";
                    _notification.SendNotification(message, reservation.UserEmail, "Reservation Confirmation");
                }

                return OperationResult<Reservation>.Success(reservation, "Reservation created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating reservation: {ex.Message}");
                return OperationResult<Reservation>.Failure("Error creating reservation");
            }
        }

        public OperationResult<Reservation> CancelReservation(Guid reservationId)
        {
            if (reservationId == Guid.Empty)
            {
                return OperationResult<Reservation>.Failure("Invalid reservation ID");
            }

            try
            {
                var reservation = _context.Reservations.Find(reservationId);
                if (reservation == null)
                {
                    return OperationResult<Reservation>.Failure("Reservation not found");
                }

                _context.Reservations.Remove(reservation);
                _context.SaveChanges();

                return OperationResult<Reservation>.Success(reservation, "Reservation cancelled successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cancelling reservation: {ex.Message}");
                return OperationResult<Reservation>.Failure("Error cancelling reservation");
            }
        }

        public OperationResult<Reservation> GetReservationById(Guid reservationId)
        {
            if (reservationId == Guid.Empty)
            {
                return OperationResult<Reservation>.Failure("Invalid reservation ID");
            }

            try
            {
                var reservation = _context.Reservations.Find(reservationId);
                if (reservation == null)
                {
                    return OperationResult<Reservation>.Failure("Reservation not found");
                }

                return OperationResult<Reservation>.Success(reservation, "Reservation retrieved successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching reservation: {ex.Message}");
                return OperationResult<Reservation>.Failure("Error fetching reservation");
            }
        }

        public IEnumerable<Reservation> GetAllReservations()
        {
            try
            {
                return _context.Reservations.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all reservations: {ex.Message}");
                return new List<Reservation>();
            }
        }

        public IEnumerable<Reservation> GetReservationsByUserId(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return new List<Reservation>();
            }

            try
            {
                return _context.Reservations.Where(r => r.UserId == userId).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching reservations by user: {ex.Message}");
                return new List<Reservation>();
            }
        }

        public OperationResult<Reservation> UpdateReservation(Guid reservationId, Reservation reservation)
        {
            if (reservationId == Guid.Empty || reservation == null)
            {
                return OperationResult<Reservation>.Failure("Invalid reservation data");
            }

            try
            {
                var existingReservation = _context.Reservations
                    .Include(r => r.User)
                    .FirstOrDefault(r => r.ReservationId == reservationId);

                if (existingReservation == null)
                {
                    return OperationResult<Reservation>.Failure("Reservation not found");
                }

                existingReservation.ReservationDate = reservation.ReservationDate;
                existingReservation.NumberOfPeople = reservation.NumberOfPeople;
                existingReservation.SpecialRequests = reservation.SpecialRequests;
                existingReservation.CustomerName = reservation.CustomerName;
                existingReservation.UserEmail = reservation.UserEmail;
                existingReservation.Occasion = reservation.Occasion;
                existingReservation.SeatingPreference = reservation.SeatingPreference;
                existingReservation.Amount = reservation.Amount;
                existingReservation.Status = reservation.Status;
                existingReservation.UpdatedAt = DateTime.UtcNow;

                _context.SaveChanges();

                return OperationResult<Reservation>.Success(existingReservation, "Reservation updated successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating reservation: {ex.Message}");
                return OperationResult<Reservation>.Failure("Error updating reservation");
            }
        }
    }
}
