using Microsoft.Extensions.DependencyInjection;
using PaymentSystem.Application.Dtos;
using PaymentSystem.Application.Interfaces;
using PaymentSystem.Application.Services;
using PaymentSystem.Infrastructure;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
builder.Services.AddSingleton<IPaymentProcessor, PaymentProcessor>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var payments = app.MapGroup("/payments");

payments.MapPost("/", async (HttpContext context) =>
{
    var processor = context.RequestServices.GetRequiredService<IPaymentProcessor>();
    var request = await context.Request.ReadFromJsonAsync<PaymentRequest>();

    if (request is null)
    {
        return (IResult)TypedResults.BadRequest(new ErrorResponse("Invalid payment payload."));
    }

    try
    {
        var payment = await processor.ProcessPaymentAsync(request);
        return TypedResults.Created($"/payments/{payment.Id}", PaymentResponse.From(payment));
    }
    catch (ArgumentException ex)
    {
        return TypedResults.BadRequest(new ErrorResponse(ex.Message));
    }
});

payments.MapGet("/", async (HttpContext context) =>
{
    var processor = context.RequestServices.GetRequiredService<IPaymentProcessor>();
    var paymentsToReturn = await processor.ListPaymentsAsync();
    return TypedResults.Ok(paymentsToReturn.Select(PaymentResponse.From));
});

payments.MapGet("/{id}", async (HttpContext context) =>
{
    var processor = context.RequestServices.GetRequiredService<IPaymentProcessor>();
    if (!Guid.TryParse(context.Request.RouteValues["id"]?.ToString(), out var id))
    {
        return (IResult)TypedResults.BadRequest(new ErrorResponse("Invalid payment id."));
    }

    var payment = await processor.GetPaymentAsync(id);
    return payment is not null
        ? TypedResults.Ok(PaymentResponse.From(payment))
        : TypedResults.NotFound();
});

payments.MapPost("/{id}/refund", async (HttpContext context) =>
{
    var processor = context.RequestServices.GetRequiredService<IPaymentProcessor>();
    if (!Guid.TryParse(context.Request.RouteValues["id"]?.ToString(), out var id))
    {
        return (IResult)TypedResults.BadRequest(new ErrorResponse("Invalid payment id."));
    }

    var request = await context.Request.ReadFromJsonAsync<RefundRequest>();
    if (request is null)
    {
        return TypedResults.BadRequest(new ErrorResponse("Invalid refund payload."));
    }

    var payment = await processor.RefundPaymentAsync(id, request.Reason);
    return payment is not null
        ? TypedResults.Ok(PaymentResponse.From(payment))
        : TypedResults.NotFound();
});

app.Run();

[JsonSerializable(typeof(PaymentResponse[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
