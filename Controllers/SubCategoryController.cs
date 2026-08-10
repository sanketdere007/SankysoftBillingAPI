using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubCategoryController : ControllerBase
{
    private readonly ISubCategoryRepository _subCategoryRepository;
    private readonly ILogger<SubCategoryController> _logger;

    public SubCategoryController(ISubCategoryRepository subCategoryRepository, ILogger<SubCategoryController> logger)
    {
        _subCategoryRepository = subCategoryRepository ?? throw new ArgumentNullException(nameof(subCategoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("InsertorUpdateSubCategory")]
    [ProducesResponseType(typeof(ApiResponse<SubCategorySaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SubCategorySaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertorUpdateSubCategory([FromBody] SubCategoryModel subCategory, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<SubCategorySaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new SubCategorySaveResult { Status = false, Message = "Validation failed.", SubCat_Id = subCategory.SubCat_Id }));
        }

        var result = await _subCategoryRepository.SaveSubCategoryAsync(subCategory, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("GetAllSubCategories")]
    [ProducesResponseType(typeof(ApiResponse<List<SubCategoryListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SubCategoryListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllSubCategories([FromQuery] SubCategoryFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _subCategoryRepository.GetAllSubCategoriesAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
