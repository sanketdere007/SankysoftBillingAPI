using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesReturnEntryController : ControllerBase
{
    private readonly ISalesReturnEntryRepository _salesReturnEntryRepository;

    public SalesReturnEntryController(ISalesReturnEntryRepository salesReturnEntryRepository)
    {
        _salesReturnEntryRepository = salesReturnEntryRepository ?? throw new ArgumentNullException(nameof(salesReturnEntryRepository));
    }

    [HttpPost("InsertOrUpdateSalesReturnEntry")]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnEntrySaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnEntrySaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertOrUpdateSalesReturnEntry([FromBody] SalesReturnEntrySaveRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<SalesReturnEntrySaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new SalesReturnEntrySaveResult { Status = false, Message = "Validation failed.", SalesReturnMaster_Id = 0 }));
        }

        var result = await _salesReturnEntryRepository.SaveSalesReturnEntryAsync(request, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
    [HttpDelete("DeleteSalesReturnEntry/{salesReturnMasterId}")]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnEntryDeleteResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalesReturnEntryDeleteResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteSalesReturnEntry(int salesReturnMasterId, CancellationToken cancellationToken = default)
    {
        var result = await _salesReturnEntryRepository.DeleteSalesReturnEntryAsync(salesReturnMasterId, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("GetSalesReturnList")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalesReturnListResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalesReturnListResponse>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSalesReturnList([FromBody] SalesReturnListRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _salesReturnEntryRepository.GetSalesReturnListAsync(request, cancellationToken);
        
        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("GetSalesReturnEntryDetailList/{salesReturnMasterId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalesReturnEntryDetailResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SalesReturnEntryDetailResponse>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSalesReturnEntryDetailList(int salesReturnMasterId, CancellationToken cancellationToken = default)
    {
        var result = await _salesReturnEntryRepository.GetSalesReturnEntryDetailListAsync(salesReturnMasterId, cancellationToken);
        
        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
