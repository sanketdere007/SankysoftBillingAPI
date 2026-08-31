using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly ILogger<SupplierController> _logger;

    public SupplierController(ISupplierRepository supplierRepository, ILogger<SupplierController> logger)
    {
        _supplierRepository = supplierRepository ?? throw new ArgumentNullException(nameof(supplierRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("InsertorUpdateSupplier")]
    [ProducesResponseType(typeof(ApiResponse<SupplierSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SupplierSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertorUpdateSupplier([FromBody] SupplierModel supplier, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<SupplierSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new SupplierSaveResult { Status = false, Message = "Validation failed.", Supp_Id = supplier.Supp_Id }));
        }

        var result = await _supplierRepository.SaveSupplierAsync(supplier, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("GetAllSuppliers")]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllSuppliers([FromQuery] SupplierFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _supplierRepository.GetAllSuppliersAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
    [HttpGet("GetSupplierPendingInvoice")]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierPendingInvoiceModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierPendingInvoiceModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSupplierPendingInvoice([FromQuery] SupplierPendingInvoiceFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _supplierRepository.GetPendingInvoicesAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("GetSupplierOutstandingReport")]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierOutstandingReportModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<SupplierOutstandingReportModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSupplierOutstandingReport([FromQuery] SupplierOutstandingReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _supplierRepository.GetOutstandingReportAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
