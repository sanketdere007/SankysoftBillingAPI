using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BatchController : ControllerBase
{
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<BatchController> _logger;

    public BatchController(IBatchRepository batchRepository, ILogger<BatchController> logger)
    {
        _batchRepository = batchRepository ?? throw new ArgumentNullException(nameof(batchRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("GetAllBatches")]
    [ProducesResponseType(typeof(ApiResponse<List<BatchListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<BatchListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllBatches([FromQuery] BatchFilterDto filter, CancellationToken cancellationToken = default)
    {
        if (filter == null)
        {
            return BadRequest(ApiResponse<List<BatchListModel>>.FailureResult(
                message: "Filter parameters are required.",
                error: "Filter cannot be null."));
        }

        var result = await _batchRepository.GetAllBatchesAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
