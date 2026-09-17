using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptEntryController : ControllerBase
{
    private readonly IReceiptEntryRepository _receiptEntryRepository;

    public ReceiptEntryController(IReceiptEntryRepository receiptEntryRepository)
    {
        _receiptEntryRepository = receiptEntryRepository ?? throw new ArgumentNullException(nameof(receiptEntryRepository));
    }

    [HttpPost("InsertOrUpdateReceiptEntry")]
    [ProducesResponseType(typeof(ApiResponse<ReceiptEntrySaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReceiptEntrySaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertOrUpdateReceiptEntry([FromBody] ReceiptEntrySaveRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<ReceiptEntrySaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new ReceiptEntrySaveResult { Status = false, Message = "Validation failed.", ReceiptMaster_Id = 0 }));
        }

        var result = await _receiptEntryRepository.SaveReceiptEntryAsync(request, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("CollectionReport")]
    [ProducesResponseType(typeof(ApiResponse<CollectionReportPagedResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CollectionReportPagedResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCollectionReport([FromBody] CollectionReportRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _receiptEntryRepository.GetCollectionReportAsync(request, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpDelete("DeleteReceiptEntry/{id}")]
    [ProducesResponseType(typeof(ApiResponse<ReceiptEntryDeleteResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ReceiptEntryDeleteResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteReceiptEntry(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponse<ReceiptEntryDeleteResult>.FailureResult(
                message: "Invalid receipt ID.",
                error: "Receipt ID must be greater than zero.",
                data: new ReceiptEntryDeleteResult { Status = false, Message = "Invalid receipt ID.", ReceiptMaster_Id = id }));
        }

        var result = await _receiptEntryRepository.DeleteReceiptEntryAsync(id, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
