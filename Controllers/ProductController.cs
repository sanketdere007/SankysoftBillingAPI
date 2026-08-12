using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductRepository productRepository, ILogger<ProductController> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("InsertOrUpdateProduct")]
    [ProducesResponseType(typeof(ApiResponse<ProductSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertOrUpdateProduct([FromBody] ProductModel product, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<ProductSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new ProductSaveResult { Status = false, Message = "Validation failed.", Prod_Id = product.Prod_Id }));
        }

        var result = await _productRepository.SaveProductAsync(product, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("GetAllProducts")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<ProductListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllProducts([FromQuery] ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        if (filter == null)
        {
            return BadRequest(ApiResponse<List<ProductListModel>>.FailureResult(
                message: "Filter parameters are required.",
                error: "Filter cannot be null."));
        }

        var result = await _productRepository.GetAllProductsAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
