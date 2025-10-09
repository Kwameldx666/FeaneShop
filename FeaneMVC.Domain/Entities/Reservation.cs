using FeaneMVC.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FeaneMVC.Domain.Entities
{
    public class Reservation
    {
        [Key]
        public Guid ReservationId { get; set; } // Primary key

        public Guid UserId { get; set; }
        public UserData? User { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        public string CustomerName { get; set; } // Replaces Name and UserEmail for consistency

        [Required(ErrorMessage = "Reservation date is required.")]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Number of people is required.")]
        [Range(1, 20, ErrorMessage = "Number of people must be between 1 and 20.")]
        public int NumberOfPeople { get; set; }

        [Required(ErrorMessage = "Contact information is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } // This could be either phone or email
        public string UserEmail { get; set; } // This could be either phone or email
        public string? SpecialRequests { get; set; }

        public string? Occasion { get; set; }

        public string? SeatingPreference { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public ReservationStatus Status { get; set; }


        [Range(0, double.MaxValue, ErrorMessage = "Amount must be a positive value.")]
        public decimal Amount { get; set; }

        public Guid TransactionId { get; set; } // Optional, based on whether a payment was made

    }
}
