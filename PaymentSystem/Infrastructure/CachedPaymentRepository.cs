using Microsoft.Extensions.Caching.Memory;
using PaymentSystem.Application.Interfaces;
using PaymentSystem.Domain;

namespace PaymentSystem.Infrastructure;

public sealed class CachedPaymentRepository : IPaymentRepository
{
    private const string AllPaymentsCacheKey = "payments:all";
    private readonly IPaymentRepository _inner;
    private readonly IMemoryCache _cache;

    public CachedPaymentRepository(IPaymentRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task AddAsync(Payment payment)
    {
        await _inner.AddAsync(payment);
        _cache.Remove(AllPaymentsCacheKey);
        _cache.Set(GetPaymentCacheKey(payment.Id), payment, TimeSpan.FromMinutes(5));
    }

    public async Task<Payment?> GetAsync(Guid id)
    {
        return await _cache.GetOrCreateAsync(GetPaymentCacheKey(id), async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return await _inner.GetAsync(id);
        });
    }

    public async Task UpdateAsync(Payment payment)
    {
        await _inner.UpdateAsync(payment);
        _cache.Remove(AllPaymentsCacheKey);
        _cache.Set(GetPaymentCacheKey(payment.Id), payment, TimeSpan.FromMinutes(5));
    }

    public async Task<IReadOnlyCollection<Payment>> ListAsync()
    {
        return await _cache.GetOrCreateAsync(AllPaymentsCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3);
            return await _inner.ListAsync();
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _inner.DeleteAsync(id);
        _cache.Remove(AllPaymentsCacheKey);
        _cache.Remove(GetPaymentCacheKey(id));
    }

    private static string GetPaymentCacheKey(Guid id) => $"payments:{id:N}";
}
