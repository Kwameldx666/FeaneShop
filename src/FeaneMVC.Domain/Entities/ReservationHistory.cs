using FeaneMVC.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FeaneMVC.Domain.Entities
{
    public class ReservationHistory
    {
        [Key]
        public Guid Id { get; set; } // Первичный ключ

        public Guid UserId { get; set; }
        public UserData User { get; set; } // Навигационное свойство для связи с UserData


        public DateTime ReservationDate { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
