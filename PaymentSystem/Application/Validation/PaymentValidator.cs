using PaymentSystem.Application.Dtos;
using PaymentSystem.Domain;

namespace PaymentSystem.Application.Validation;

public sealed class PaymentValidator
{
    public void Validate(PaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(request.Amount));
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            throw new ArgumentException("Currency is required.", nameof(request.Currency));
        }

        if (!Enum.TryParse<PaymentMethod>(request.Method, true, out _))
        {
            throw new ArgumentException($"Unsupported payment method '{request.Method}'.", nameof(request.Method));
        }
    }
}
