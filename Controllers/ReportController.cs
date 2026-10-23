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

    [HttpGet("ProductWiseSalesReturnReport")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<ProductWiseSalesReturnReportModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<ProductWiseSalesReturnReportModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductWiseSalesReturnReport([FromQuery] ProductWiseSalesReturnReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _reportRepository.GetProductWiseSalesReturnReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("ProductWiseSalesReturnDetailReport")]
    [ProducesResponseType(typeof(ApiResponse<ProductWiseSalesReturnDetailReportModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductWiseSalesReturnDetailReportModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ProductWiseSalesReturnDetailReportModel>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductWiseSalesReturnDetailReport([FromQuery] ProductWiseSalesReturnDetailReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        if (filter.ProductId <= 0)
        {
            return BadRequest(ApiResponse<ProductWiseSalesReturnDetailReportModel>.FailureResult(
                message: "Valid ProductId is required.",
                error: "ProductId must be greater than 0.",
                data: new ProductWiseSalesReturnDetailReportModel()));
        }

        var result = await _reportRepository.GetProductWiseSalesReturnDetailReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("CustomerWiseSalesReturnReport")]
    [ProducesResponseType(typeof(ApiResponse<CustomerWiseSalesReturnReportModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerWiseSalesReturnReportModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CustomerWiseSalesReturnReportModel>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCustomerWiseSalesReturnReport([FromQuery] CustomerWiseSalesReturnReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate.Value.Date > filter.ToDate.Value.Date)
        {
            return BadRequest(ApiResponse<CustomerWiseSalesReturnReportModel>.FailureResult(
                message: "From Date cannot be greater than To Date.",
                error: "From Date cannot be greater than To Date.",
                data: new CustomerWiseSalesReturnReportModel()));
        }

        var result = await _reportRepository.GetCustomerWiseSalesReturnReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("ProductWisePurchaseReport")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<ProductWisePurchaseReportModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<ProductWisePurchaseReportModel>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<ProductWisePurchaseReportModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductWisePurchaseReport([FromQuery] ProductWisePurchaseReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate.Value.Date > filter.ToDate.Value.Date)
        {
            return BadRequest(ApiResponse<PagedListResult<ProductWisePurchaseReportModel>>.FailureResult(
                message: "From Date cannot be greater than To Date.",
                error: "From Date cannot be greater than To Date.",
                data: new PagedListResult<ProductWisePurchaseReportModel>()));
        }

        var result = await _reportRepository.GetProductWisePurchaseReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("StockLedgerReport")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<StockLedgerReportModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<StockLedgerReportModel>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<StockLedgerReportModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStockLedgerReport([FromQuery] StockLedgerReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate.Value.Date > filter.ToDate.Value.Date)
        {
            return BadRequest(ApiResponse<PagedListResult<StockLedgerReportModel>>.FailureResult(
                message: "From Date cannot be greater than To Date.",
                error: "From Date cannot be greater than To Date.",
                data: new PagedListResult<StockLedgerReportModel>()));
        }

        var result = await _reportRepository.GetStockLedgerReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    [HttpGet("LowStockReport")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<LowStockReportModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<LowStockReportModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLowStockReport([FromQuery] LowStockReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _reportRepository.GetLowStockReportAsync(filter, cancellationToken);
        if (result.Status)
        {
            return Ok(result);
        }
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
