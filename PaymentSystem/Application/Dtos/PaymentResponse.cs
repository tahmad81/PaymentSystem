using PaymentSystem.Domain;

namespace PaymentSystem.Application.Dtos;

public sealed record PaymentResponse(
    Guid Id,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    string? Reference,
    string? Description,
    DateTime CreatedAt,
    DateTime? ProcessedAt,
    string? FailureReason)
{
    public static PaymentResponse From(Payment payment) => new(
        payment.Id,
        payment.Amount,
        payment.Currency,
        payment.Method.ToString(),
        payment.Status.ToString(),
        payment.Reference,
        payment.Description,
        payment.CreatedAt,
        payment.ProcessedAt,
        payment.FailureReason);
}
