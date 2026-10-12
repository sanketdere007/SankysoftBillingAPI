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

    [HttpGet("OutstandingReceivableReport")]
    [ProducesResponseType(typeof(ApiResponse<OutstandingReceivableReportModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OutstandingReceivableReportModel>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOutstandingReceivableReport([FromQuery] OutstandingReceivableReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _reportRepository.GetOutstandingReceivableReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("PaymentEntryReport")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<PaymentEntryReportModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<PaymentEntryReportModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPaymentEntryReport([FromQuery] PaymentEntryReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _reportRepository.GetPaymentEntryReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("BatchWiseStockReport")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<BatchWiseStockReportModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<BatchWiseStockReportModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBatchWiseStockReport([FromQuery] BatchWiseStockReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _reportRepository.GetBatchWiseStockReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("CustomerWiseSalesReport")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<CustomerWiseSalesReportModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<CustomerWiseSalesReportModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCustomerWiseSalesReport([FromQuery] CustomerWiseSalesReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _reportRepository.GetCustomerWiseSalesReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
