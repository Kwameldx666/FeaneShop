using FeaneMVC.Application.Common.Interfaces.Persistence;
using FeaneMVC.Domain.Entities;
using FeaneMVC.Infrastructure.Persistence.Db;
using Microsoft.EntityFrameworkCore;

namespace FeaneMVC.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UserData?> FindUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public async Task<Cart?> FindCartWithItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cart
            .Include(cart => cart.CartItems)
            .FirstOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);
    }

    public void RemoveCart(Cart cart)
    {
        _dbContext.Cart.Remove(cart);
    }

    public void AddPaymentRecord(PaymentRecord paymentRecord)
    {
        _dbContext.PaymentRecords.Add(paymentRecord);
    }

    public Task<PaymentRecord?> FindPaymentByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return _dbContext.PaymentRecords.FirstOrDefaultAsync(record => record.TransactionId == transactionId, cancellationToken);
    }

}
