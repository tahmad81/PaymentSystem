using PaymentSystem.Domain;

namespace PaymentSystem.Application.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<Payment?> GetAsync(Guid id);
    Task UpdateAsync(Payment payment);
    Task<IReadOnlyCollection<Payment>> ListAsync();
}
