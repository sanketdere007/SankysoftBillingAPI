using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GSTTaxController : ControllerBase
{
    private readonly IGSTTaxRepository _gstTaxRepository;
    private readonly ILogger<GSTTaxController> _logger;

    public GSTTaxController(IGSTTaxRepository gstTaxRepository, ILogger<GSTTaxController> logger)
    {
        _gstTaxRepository = gstTaxRepository ?? throw new ArgumentNullException(nameof(gstTaxRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("InsertOrUpdateGSTTax")]
    [ProducesResponseType(typeof(ApiResponse<GSTTaxSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GSTTaxSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertOrUpdateGSTTax([FromBody] GSTTaxModel gstTax, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<GSTTaxSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new GSTTaxSaveResult { Status = false, Message = "Validation failed.", GSTTax_Id = gstTax.GSTTax_Id }));
        }

        var result = await _gstTaxRepository.SaveGSTTaxAsync(gstTax, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("GetAllGSTTaxes")]
    [ProducesResponseType(typeof(ApiResponse<List<GSTTaxListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<GSTTaxListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllGSTTaxes([FromQuery] GSTTaxFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _gstTaxRepository.GetAllGSTTaxesAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
