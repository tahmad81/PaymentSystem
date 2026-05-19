using Microsoft.AspNetCore.Mvc;
using PaymentSystem.Application.Dtos;
using PaymentSystem.Application.Interfaces;
using PaymentSystem.Domain;

namespace CleanTest.Controllers;

[ApiController]
[Route("payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentProcessor _processor;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentProcessor processor, ILogger<PaymentsController> logger)
    {
        _processor = processor;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> CreatePayment([FromBody] PaymentRequest request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse("Invalid payment payload."));
        }

        try
        {
            var payment = await _processor.ProcessPaymentAsync(request);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, PaymentResponse.From(payment));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid payment request.");
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetPayments()
    {
        var payments = await _processor.ListPaymentsAsync();
        return Ok(payments.Select(PaymentResponse.From));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse>> GetPayment(Guid id)
    {
        var payment = await _processor.GetPaymentAsync(id);
        if (payment is null)
        {
            return NotFound();
        }

        return Ok(PaymentResponse.From(payment));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PaymentResponse>> UpdatePayment(Guid id, [FromBody] PaymentRequest request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse("Invalid update payload."));
        }

        var updated = await _processor.UpdatePaymentAsync(id, request);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(PaymentResponse.From(updated));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePayment(Guid id)
    {
        var deleted = await _processor.DeletePaymentAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<PaymentResponse>> RefundPayment(Guid id, [FromBody] RefundRequest request)
    {
        if (request is null)
        {
            return BadRequest(new ErrorResponse("Invalid refund payload."));
        }

        var payment = await _processor.RefundPaymentAsync(id, request.Reason);
        if (payment is null)
        {
            return NotFound();
        }

        return Ok(PaymentResponse.From(payment));
    }
}
