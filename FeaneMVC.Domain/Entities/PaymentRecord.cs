namespace FeaneMVC.Domain.Entities;

public class PaymentRecord
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; }
    public string CardHolderName { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string CVV { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string TransactionId { get; set; }
    public DateTime DateProcessed { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime? DateRefunded { get; set; }
}
