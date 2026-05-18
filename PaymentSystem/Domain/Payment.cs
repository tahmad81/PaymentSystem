namespace PaymentSystem.Domain;

public sealed record Payment(
    Guid Id,
    decimal Amount,
    string Currency,
    PaymentMethod Method,
    PaymentStatus Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt = null,
    string? Reference = null,
    string? Description = null,
    string? FailureReason = null);
