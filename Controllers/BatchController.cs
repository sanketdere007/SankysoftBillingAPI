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

    [HttpGet("GetAllProductStock")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductStockModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<ProductStockModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllProductStock([FromQuery] ProductStockFilterDto filter, CancellationToken cancellationToken = default)
    {
        if (filter == null)
        {
            return BadRequest(ApiResponse<List<ProductStockModel>>.FailureResult(
                message: "Filter parameters are required.",
                error: "Filter cannot be null."));
        }

        var result = await _batchRepository.GetAllProductStockAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpPost("InsertOrUpdateBatch")]
    [ProducesResponseType(typeof(ApiResponse<BatchSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BatchSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertOrUpdateBatch([FromBody] BatchSaveModel batch, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<BatchSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new BatchSaveResult { Status = false, Message = "Validation failed.", Batch_Id = batch.Batch_Id }));
        }

        var result = await _batchRepository.SaveBatchAsync(batch, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
