using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseEntryController : ControllerBase
{
    private readonly IPurchaseEntryRepository _purchaseEntryRepository;

    public PurchaseEntryController(IPurchaseEntryRepository purchaseEntryRepository)
    {
        _purchaseEntryRepository = purchaseEntryRepository ?? throw new ArgumentNullException(nameof(purchaseEntryRepository));
    }

    [HttpPost("InsertOrUpdatePurchaseEntry")]
    [ProducesResponseType(typeof(ApiResponse<PurchaseEntrySaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PurchaseEntrySaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertOrUpdatePurchaseEntry([FromBody] PurchaseEntrySaveRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<PurchaseEntrySaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new PurchaseEntrySaveResult { Status = false, Message = "Validation failed.", PurchaseMaster_Id = 0 }));
        }

        var result = await _purchaseEntryRepository.SavePurchaseEntryAsync(request, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
