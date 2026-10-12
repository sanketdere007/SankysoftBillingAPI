using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseReturnEntryController : ControllerBase
{
    private readonly IPurchaseReturnEntryRepository _purchaseReturnEntryRepository;

    public PurchaseReturnEntryController(IPurchaseReturnEntryRepository purchaseReturnEntryRepository)
    {
        _purchaseReturnEntryRepository = purchaseReturnEntryRepository ?? throw new ArgumentNullException(nameof(purchaseReturnEntryRepository));
    }

    [HttpPost("InsertOrUpdatePurchaseReturnEntry")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnEntrySaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseReturnEntrySaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertOrUpdatePurchaseReturnEntry([FromBody] PurchaseReturnEntrySaveRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<PurchaseReturnEntrySaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new PurchaseReturnEntrySaveResult { Status = false, Message = "Validation failed.", PurchaseReturnMaster_Id = 0 }));
        }

        var result = await _purchaseReturnEntryRepository.SavePurchaseReturnEntryAsync(request, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("GetPurchaseReturnMasterViewList")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<PurchaseReturnMasterViewListResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<PurchaseReturnMasterViewListResponse>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPurchaseReturnMasterViewList([FromBody] PurchaseReturnMasterViewListRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _purchaseReturnEntryRepository.GetPurchaseReturnMasterViewListAsync(request, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    [HttpPost("GetPurchaseReturnDetailViewList")]
    [ProducesResponseType(typeof(ApiResponse<List<PurchaseReturnDetailViewListResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<PurchaseReturnDetailViewListResponse>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPurchaseReturnDetailViewList([FromBody] PurchaseReturnDetailViewListRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _purchaseReturnEntryRepository.GetPurchaseReturnDetailViewListAsync(request, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
