using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaymentSystem.Application.Interfaces;
using PaymentSystem.Domain;

namespace PaymentSystem.Infrastructure;

public sealed class EfPaymentRepository : IPaymentRepository
{
    private readonly PaymentDbContext _dbContext;
    private readonly ILogger<EfPaymentRepository> _logger;

    public EfPaymentRepository(PaymentDbContext dbContext, ILogger<EfPaymentRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task AddAsync(Payment payment)
    {
        _logger.LogInformation("Adding payment {PaymentId} for {Amount} {Currency}.", payment.Id, payment.Amount, payment.Currency);
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Payment?> GetAsync(Guid id)
    {
        _logger.LogDebug("Loading payment {PaymentId} from database.", id);
        return await _dbContext.Payments.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id);
    }

    public async Task UpdateAsync(Payment payment)
    {
        DetachTrackedPayment(payment.Id);
        _logger.LogInformation("Updating payment {PaymentId} to status {Status}.", payment.Id, payment.Status);
        _dbContext.Payments.Update(payment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Payment>> ListAsync()
    {
        _logger.LogDebug("Listing all payments from database.");
        return await _dbContext.Payments.AsNoTracking().ToArrayAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting payment {PaymentId}.", id);
        var payment = await _dbContext.Payments.FindAsync(id);
        if (payment is null)
        {
            return;
        }

        _dbContext.Payments.Remove(payment);
        await _dbContext.SaveChangesAsync();
    }

    private void DetachTrackedPayment(Guid id)
    {
        var trackedEntry = _dbContext.ChangeTracker.Entries<Payment>().FirstOrDefault(e => e.Entity.Id == id);
        if (trackedEntry is not null)
        {
            _logger.LogDebug("Detaching existing tracked payment {PaymentId} before update.", id);
            trackedEntry.State = EntityState.Detached;
        }
    }
}
