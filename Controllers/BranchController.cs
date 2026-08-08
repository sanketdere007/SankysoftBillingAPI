using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Branch Controller managing branch master data retrieval.
/// Protected by JWT Authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchController : ControllerBase
{
    private readonly IBranchRepository _branchRepository;
    private readonly ILogger<BranchController> _logger;

    public BranchController(IBranchRepository branchRepository, ILogger<BranchController> logger)
    {
        _branchRepository = branchRepository ?? throw new ArgumentNullException(nameof(branchRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get All Branches (using SP_Branch_GetAll)
    /// Fetches branch records from SQL Server via SP_Branch_GetAll with optional company and active status filters.
    /// </summary>
    /// <param name="filter">Optional query filter parameters: Branch_CompId / CompId, Branch_IsActive / IsActive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of branches matching filter criteria</returns>
  
    [HttpGet("GetAllBranches")]
    [ProducesResponseType(typeof(ApiResponse<List<BranchListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<BranchListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllBranch([FromQuery] BranchFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _branchRepository.GetAllBranchesAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
