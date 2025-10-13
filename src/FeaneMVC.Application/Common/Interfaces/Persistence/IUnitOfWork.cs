namespace FeaneMVC.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork
{
    ICartRepository Carts { get; }

    IDishReadRepository DishReader { get; }

    IDishWriteRepository DishWriter { get; }

    IPaymentRepository Payments { get; }

    ISessionRepository Sessions { get; }

    IAnalyticsRepository Analytics { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
