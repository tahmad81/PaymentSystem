using PaymentSystem.Application.Dtos;
using PaymentSystem.Domain;

namespace PaymentSystem.Application.Interfaces;

public interface IPaymentProcessor
{
    Task<Payment> ProcessPaymentAsync(PaymentRequest request);
    Task<Payment?> RefundPaymentAsync(Guid paymentId, string reason);
    Task<Payment?> GetPaymentAsync(Guid paymentId);
    Task<IReadOnlyCollection<Payment>> ListPaymentsAsync();
}
