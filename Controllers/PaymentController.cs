using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentRepository paymentRepository, ILogger<PaymentController> logger)
    {
        _paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("InsertOrUpdatePayment")]
    [ProducesResponseType(typeof(ApiResponse<PaymentSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertOrUpdatePayment([FromBody] PaymentModel payment, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<PaymentSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new PaymentSaveResult { Status = false, Message = "Validation failed.", PaymentMaster_Id = payment.PaymentMaster_Id }));
        }

        var result = await _paymentRepository.SavePaymentEntryAsync(payment, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpDelete("DeletePayment/{paymentMasterId}")]
    [ProducesResponseType(typeof(ApiResponse<PaymentEntryDeleteResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PaymentEntryDeleteResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeletePayment(int paymentMasterId, CancellationToken cancellationToken = default)
    {
        if (paymentMasterId <= 0)
        {
            return BadRequest(ApiResponse<PaymentEntryDeleteResult>.FailureResult(
                message: "Invalid PaymentMaster_Id.",
                error: "PaymentMaster_Id must be greater than zero.",
                data: new PaymentEntryDeleteResult { Status = false, Message = "Invalid PaymentMaster_Id.", PaymentMaster_Id = paymentMasterId }));
        }

        var result = await _paymentRepository.DeletePaymentEntryAsync(paymentMasterId, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
