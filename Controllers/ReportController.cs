using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportRepository _reportRepository;

    public ReportController(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
    }

    [HttpGet("ProductWiseSalesReport")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<ProductWiseSalesReportModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<ProductWiseSalesReportModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductWiseSalesReport([FromQuery] ProductWiseSalesReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _reportRepository.GetProductWiseSalesReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("ProductWiseCustomerPurchaseList")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<ProductWiseCustomerPurchaseListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<ProductWiseCustomerPurchaseListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductWiseCustomerPurchaseList([FromQuery] ProductWiseCustomerPurchaseListFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _reportRepository.GetProductWiseCustomerPurchaseListAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
