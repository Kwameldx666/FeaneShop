using FeaneMVC.Application.Common.Interfaces.Services;
using FeaneMVC.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace FeaneMVC.Application.Services
{
    public class ReservationNotificationService : IReservationNotifier
    {
        private readonly string _smtpServer;
        private readonly string _fromEmail;
        private readonly ILogger<ReservationNotificationService> _logger;

        public ReservationNotificationService(string smtpServer, string fromEmail, ILogger<ReservationNotificationService> logger)
        {
            _smtpServer = smtpServer;
            _fromEmail = fromEmail;
            _logger = logger;
        }

        public void SendReservationConfirmation(Reservation reservation)
        {
            SendEmail(reservation.UserEmail, "Reservation Confirmation", $"Your reservation for {reservation.ReservationDate} has been confirmed.");
        }

        public void SendReservationCancellation(Reservation reservation)
        {
            SendEmail(reservation.UserEmail, "Reservation Cancellation", $"Your reservation for {reservation.ReservationDate} has been canceled.");
        }

        private void SendEmail(string toEmail, string subject, string body)
        {
            using var client = new SmtpClient(_smtpServer);
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = body
            };
            mailMessage.To.Add(toEmail);

            try
            {
                client.Send(mailMessage);
                _logger.LogInformation("Reservation notification sent to {Recipient}", toEmail);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error sending reservation email to {Recipient}", toEmail);
            }
        }
    }
}
