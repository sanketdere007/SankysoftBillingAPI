using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// State Controller managing state master data retrieval.
/// Protected by JWT Authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StateController : ControllerBase
{
    private readonly IStateRepository _stateRepository;
    private readonly ILogger<StateController> _logger;

    public StateController(IStateRepository stateRepository, ILogger<StateController> logger)
    {
        _stateRepository = stateRepository ?? throw new ArgumentNullException(nameof(stateRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get All States (using SP_State_GetAll)
    /// Fetches state records from SQL Server via SP_State_GetAll with optional search and active status filters.
    /// </summary>
    /// <param name="filter">Optional query filter parameters: Search, IsActive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of states matching filter criteria</returns>
    [HttpGet("GetAllStates")]
    [ProducesResponseType(typeof(ApiResponse<List<StateListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<StateListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllStates([FromQuery] StateFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _stateRepository.GetAllStatesAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
