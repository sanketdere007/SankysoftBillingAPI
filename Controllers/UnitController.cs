using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitController : ControllerBase
{
    private readonly IUnitRepository _unitRepository;
    private readonly ILogger<UnitController> _logger;

    public UnitController(IUnitRepository unitRepository, ILogger<UnitController> logger)
    {
        _unitRepository = unitRepository ?? throw new ArgumentNullException(nameof(unitRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("InsertorUpdateUnit")]
    [ProducesResponseType(typeof(ApiResponse<UnitSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UnitSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertorUpdateUnit([FromBody] UnitModel unit, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<UnitSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new UnitSaveResult { Status = false, Message = "Validation failed.", Unit_Id = unit.Unit_Id }));
        }

        var result = await _unitRepository.SaveUnitAsync(unit, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("GetAllUnits")]
    [ProducesResponseType(typeof(ApiResponse<List<UnitListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<UnitListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllUnits([FromQuery] UnitFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _unitRepository.GetAllUnitsAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
