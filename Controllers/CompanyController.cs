using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Company Controller managing company master data retrieval.
/// Protected by JWT Authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ICompanyRepository companyRepository, ILogger<CompanyController> logger)
    {
        _companyRepository = companyRepository ?? throw new ArgumentNullException(nameof(companyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get All Companies (using SP_Company_GetAll)
    /// Fetches company records from SQL Server via SP_Company_GetAll with optional active status filter.
    /// </summary>
    /// <param name="filter">Optional query filter parameters: Comp_IsActive / IsActive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of companies matching filter criteria</returns>

    [HttpGet("GetAllCompanies")]
    [ProducesResponseType(typeof(ApiResponse<List<CompanyListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<CompanyListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllCompany([FromQuery] CompanyFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _companyRepository.GetAllCompaniesAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
