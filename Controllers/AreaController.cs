using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Area Controller managing area master registration, updates, and area list retrieval.
/// Protected by JWT Authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AreaController : ControllerBase
{
    private readonly IAreaRepository _areaRepository;
    private readonly ILogger<AreaController> _logger;

    public AreaController(IAreaRepository areaRepository, ILogger<AreaController> logger)
    {
        _areaRepository = areaRepository ?? throw new ArgumentNullException(nameof(areaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// API 1: Insert or Update Area (using SP_Area_InsertOrUpdate)
    /// Converts Area model to JSON and passes it to the @AreaJsonData stored procedure parameter.
    /// If Area_Id = 0 -> Performs Insert.
    /// If Area_Id > 0 -> Performs Update.
    /// </summary>
    /// <param name="area">Area data payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status and Message</returns>
    [HttpPost("InsertorUpdateArea")]
    [ProducesResponseType(typeof(ApiResponse<AreaSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AreaSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertorUpdateArea([FromBody] AreaModel area, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<AreaSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new AreaSaveResult { Status = false, Message = "Validation failed.", Area_Id = area.Area_Id }));
        }

        var result = await _areaRepository.SaveAreaAsync(area, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// API 2: Get All Areas (using SP_Area_GetAll)
    /// Fetches area records from SQL Server via SP_Area_GetAll with optional search and filters.
    /// </summary>
    /// <param name="filter">Optional query filter parameters: Search, StateId, CityId, Pincode, IsActive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of areas matching filter criteria</returns>
    [HttpGet("GetAllAreas")]
    [ProducesResponseType(typeof(ApiResponse<List<AreaListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<AreaListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAreas([FromQuery] AreaFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _areaRepository.GetAllAreasAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
