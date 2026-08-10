using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrandController : ControllerBase
{
    private readonly IBrandRepository _brandRepository;
    private readonly ILogger<BrandController> _logger;

    public BrandController(IBrandRepository brandRepository, ILogger<BrandController> logger)
    {
        _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("InsertorUpdateBrand")]
    [ProducesResponseType(typeof(ApiResponse<BrandSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BrandSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertorUpdateBrand([FromBody] BrandModel brand, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<BrandSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new BrandSaveResult { Status = false, Message = "Validation failed.", Brand_Id = brand.Brand_Id }));
        }

        var result = await _brandRepository.SaveBrandAsync(brand, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("GetAllBrands")]
    [ProducesResponseType(typeof(ApiResponse<List<BrandListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<BrandListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllBrands([FromQuery] BrandFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _brandRepository.GetAllBrandsAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
