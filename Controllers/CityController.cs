using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// City Controller managing city master registration, updates, and city list retrieval.
/// Protected by JWT Authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CityController : ControllerBase
{
    private readonly ICityRepository _cityRepository;
    private readonly ILogger<CityController> _logger;

    public CityController(ICityRepository cityRepository, ILogger<CityController> logger)
    {
        _cityRepository = cityRepository ?? throw new ArgumentNullException(nameof(cityRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// API 1: Insert or Update City (using SP_City_InsertOrUpdate)
    /// Converts City model to JSON and passes it to the @CityJsonData stored procedure parameter.
    /// If City_Id = 0 -> Performs Insert.
    /// If City_Id > 0 -> Performs Update.
    /// </summary>
    /// <param name="city">City data payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status and Message</returns>
    [HttpPost("InsertorUpdateCity")]
    [ProducesResponseType(typeof(ApiResponse<CitySaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CitySaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertorUpdateCity([FromBody] CityModel city, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<CitySaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new CitySaveResult { Status = false, Message = "Validation failed.", City_Id = city.City_Id }));
        }

        var result = await _cityRepository.SaveCityAsync(city, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// API 2: Get All Cities (using SP_City_GetAll)
    /// Fetches city records from SQL Server via SP_City_GetAll with optional search and filters.
    /// </summary>
    /// <param name="filter">Optional query filter parameters: Search, StateId, IsActive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of cities matching filter criteria</returns>
    [HttpGet("GetAllCities")]
    [ProducesResponseType(typeof(ApiResponse<List<CityListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<CityListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllCities([FromQuery] CityFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _cityRepository.GetAllCitiesAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
