using Microsoft.Extensions.Logging;
using PaymentSystem.Application.Dtos;
using PaymentSystem.Application.Interfaces;
using PaymentSystem.Application.Validation;
using PaymentSystem.Domain;

namespace PaymentSystem.Application.Services;

public sealed class PaymentProcessor : IPaymentProcessor
{
    private readonly IPaymentRepository _repository;
    private readonly PaymentValidator _validator;
    private readonly ILogger<PaymentProcessor> _logger;

    public PaymentProcessor(IPaymentRepository repository, ILogger<PaymentProcessor> logger)
    {
        _repository = repository;
        _validator = new PaymentValidator();
        _logger = logger;
    }

    public async Task<Payment> ProcessPaymentAsync(PaymentRequest request)
    {
        _validator.Validate(request);
        _logger.LogInformation("Processing payment request for {Amount} {Currency} via {Method}.", request.Amount, request.Currency, request.Method);

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

    public async Task<Payment?> UpdatePaymentAsync(Guid paymentId, PaymentRequest request)
    {
        _validator.Validate(request);
        _logger.LogInformation("Updating payment {PaymentId}.", paymentId);

        var existing = await _repository.GetAsync(paymentId);
        if (existing is null || existing.Status != PaymentStatus.Pending)
        {
            _logger.LogWarning("Cannot update payment {PaymentId}; it does not exist or is not pending.", paymentId);
            return null;
        }

        var updated = existing with
        {
            Amount = request.Amount,
            Currency = request.Currency.Trim().ToUpperInvariant(),
            Method = Enum.Parse<PaymentMethod>(request.Method, true),
            Description = request.Description
        };

        await _repository.UpdateAsync(updated);
        return updated;
    }

    public async Task<bool> DeletePaymentAsync(Guid paymentId)
    {
        _logger.LogInformation("Deleting payment {PaymentId}.", paymentId);
        var existing = await _repository.GetAsync(paymentId);
        if (existing is null || existing.Status != PaymentStatus.Pending)
        {
            _logger.LogWarning("Cannot delete payment {PaymentId}; it does not exist or is not pending.", paymentId);
            return false;
        }

        await _repository.DeleteAsync(paymentId);
        return true;
    }

    public Task<Payment?> GetPaymentAsync(Guid paymentId)
    {
        _logger.LogDebug("Retrieving payment {PaymentId}.", paymentId);
        return _repository.GetAsync(paymentId);
    }

    public Task<IReadOnlyCollection<Payment>> ListPaymentsAsync()
    {
        _logger.LogDebug("Retrieving all payments.");
        return _repository.ListAsync();
    }

    public async Task<Payment?> RefundPaymentAsync(Guid paymentId, string reason)
    {
        _logger.LogInformation("Refunding payment {PaymentId}.", paymentId);
        var payment = await _repository.GetAsync(paymentId);
        if (payment is null || payment.Status != PaymentStatus.Completed)
        {
            _logger.LogWarning("Cannot refund payment {PaymentId}; it does not exist or is not completed.", paymentId);
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
