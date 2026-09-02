using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesEntryController : ControllerBase
{
    private readonly ISalesEntryRepository _salesEntryRepository;

    public SalesEntryController(ISalesEntryRepository salesEntryRepository)
    {
        _salesEntryRepository = salesEntryRepository ?? throw new ArgumentNullException(nameof(salesEntryRepository));
    }

    [HttpPost("InsertOrUpdateSalesEntry")]
    [ProducesResponseType(typeof(ApiResponse<SalesEntrySaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SalesEntrySaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertOrUpdateSalesEntry([FromBody] SalesEntrySaveRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<SalesEntrySaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new SalesEntrySaveResult { Status = false, Message = "Validation failed.", SalesMaster_Id = 0 }));
        }

        var result = await _salesEntryRepository.SaveSalesEntryAsync(request, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
