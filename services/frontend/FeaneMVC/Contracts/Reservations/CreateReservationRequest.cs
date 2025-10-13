using System.ComponentModel.DataAnnotations;

namespace FeaneMVC.Contracts.Reservations;

public class CreateReservationRequest
{
    [Required(ErrorMessage = "Введите имя клиента.")]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите номер телефона.")]
    [Phone(ErrorMessage = "Некорректный номер телефона.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите адрес электронной почты.")]
    [EmailAddress(ErrorMessage = "Некорректный адрес электронной почты.")]
    public string UserEmail { get; set; } = string.Empty;

    [Range(1, 20, ErrorMessage = "Количество гостей должно быть от 1 до 20.")]
    public int NumberOfPeople { get; set; }

    [Required(ErrorMessage = "Выберите дату и время.")]
    [DataType(DataType.DateTime)]
    public DateTime ReservationDateTime { get; set; } = DateTime.Now.AddHours(2);

    [Display(Name = "Повод визита")]
    public string? Occasion { get; set; }

    [Display(Name = "Предпочтения по рассадке")]
    public string? SeatingPreference { get; set; }

    [Display(Name = "Особые пожелания")]
    [StringLength(500)]
    public string? SpecialRequests { get; set; }

    [Range(0, 1000, ErrorMessage = "Бюджет на гостя не может быть отрицательным.")]
    [Display(Name = "Бюджет на одного гостя, BYN")]
    public decimal BudgetPerGuest { get; set; } = 20;
}
