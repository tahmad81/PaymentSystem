using PaymentSystem.Application.Dtos;
using PaymentSystem.Application.Interfaces;
using PaymentSystem.Application.Validation;
using PaymentSystem.Domain;

namespace PaymentSystem.Application.Services;

public sealed class PaymentProcessor : IPaymentProcessor
{
    private readonly IPaymentRepository _repository;
    private readonly PaymentValidator _validator;

    public PaymentProcessor(IPaymentRepository repository)
    {
        _repository = repository;
        _validator = new PaymentValidator();
    }

    public async Task<Payment> ProcessPaymentAsync(PaymentRequest request)
    {
        _validator.Validate(request);

        var method = Enum.Parse<PaymentMethod>(request.Method, true);

        var payment = new Payment(
            Guid.NewGuid(),
            request.Amount,
            request.Currency.Trim().ToUpperInvariant(),
            method,
            PaymentStatus.Pending,
            DateTime.UtcNow,
            Description: request.Description);

        await _repository.AddAsync(payment);

        var completed = CompletePayment(payment);
        await _repository.UpdateAsync(completed);
        return completed;
    }

    public Task<Payment?> GetPaymentAsync(Guid paymentId)
    {
        return _repository.GetAsync(paymentId);
    }

    public Task<IReadOnlyCollection<Payment>> ListPaymentsAsync()
    {
        return _repository.ListAsync();
    }

    public async Task<Payment?> RefundPaymentAsync(Guid paymentId, string reason)
    {
        var payment = await _repository.GetAsync(paymentId);
        if (payment is null || payment.Status != PaymentStatus.Completed)
        {
            return null;
        }

        var refunded = payment with
        {
            Status = PaymentStatus.Refunded,
            ProcessedAt = DateTime.UtcNow,
            FailureReason = string.IsNullOrWhiteSpace(reason) ? "Refund requested." : reason
        };

        await _repository.UpdateAsync(refunded);
        return refunded;
    }

    private static Payment CompletePayment(Payment payment)
    {
        var reference = $"PAY-{payment.Id:N}";

        return payment with
        {
            Status = PaymentStatus.Completed,
            ProcessedAt = DateTime.UtcNow,
            Reference = reference
        };
    }
}
