namespace PaymentSystem.Application.Dtos;

public sealed record PaymentRequest(decimal Amount, string Currency, string Method, string? Description = null);
