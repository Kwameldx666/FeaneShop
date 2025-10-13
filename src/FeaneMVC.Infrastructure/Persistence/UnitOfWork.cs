using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Infrastructure.Persistence.Db;

namespace FeaneMVC.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;

    public UnitOfWork(
        ApplicationDbContext dbContext,
        ICartRepository cartRepository,
        IDishReadRepository dishReadRepository,
        IDishWriteRepository dishWriteRepository,
        IPaymentRepository paymentRepository,
        ISessionRepository sessionRepository,
        IAnalyticsRepository analyticsRepository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Carts = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        DishReader = dishReadRepository ?? throw new ArgumentNullException(nameof(dishReadRepository));
        DishWriter = dishWriteRepository ?? throw new ArgumentNullException(nameof(dishWriteRepository));
        Payments = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        Sessions = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        Analytics = analyticsRepository ?? throw new ArgumentNullException(nameof(analyticsRepository));
    }

    public ICartRepository Carts { get; }

    public IDishReadRepository DishReader { get; }

    public IDishWriteRepository DishWriter { get; }

    public IPaymentRepository Payments { get; }

    public ISessionRepository Sessions { get; }

    public IAnalyticsRepository Analytics { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
