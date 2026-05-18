using PaymentSystem.Application.Interfaces;
using PaymentSystem.Domain;

namespace PaymentSystem.Infrastructure;

public sealed class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly Dictionary<Guid, Payment> _store = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task AddAsync(Payment payment)
    {
        await _lock.WaitAsync();
        try
        {
            _store[payment.Id] = payment;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Payment?> GetAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            _store.TryGetValue(id, out var payment);
            return payment;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateAsync(Payment payment)
    {
        await _lock.WaitAsync();
        try
        {
            _store[payment.Id] = payment;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyCollection<Payment>> ListAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return _store.Values.ToArray();
        }
        finally
        {
            _lock.Release();
        }
    }
}
