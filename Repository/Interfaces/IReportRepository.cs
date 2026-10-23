using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IReportRepository
{
    Task<ApiResponse<PagedListResult<ProductWiseSalesReportModel>>> GetProductWiseSalesReportAsync(ProductWiseSalesReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<ProductWiseCustomerPurchaseListModel>>> GetProductWiseCustomerPurchaseListAsync(ProductWiseCustomerPurchaseListFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<OutstandingReceivableReportModel>> GetOutstandingReceivableReportAsync(OutstandingReceivableReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<PaymentEntryReportModel>>> GetPaymentEntryReportAsync(PaymentEntryReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<BatchWiseStockReportModel>>> GetBatchWiseStockReportAsync(BatchWiseStockReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<CustomerWiseSalesReportModel>>> GetCustomerWiseSalesReportAsync(CustomerWiseSalesReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<ProductWiseSalesReturnReportModel>>> GetProductWiseSalesReturnReportAsync(ProductWiseSalesReturnReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductWiseSalesReturnDetailReportModel>> GetProductWiseSalesReturnDetailReportAsync(ProductWiseSalesReturnDetailReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerWiseSalesReturnReportModel>> GetCustomerWiseSalesReturnReportAsync(CustomerWiseSalesReturnReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<ProductWisePurchaseReportModel>>> GetProductWisePurchaseReportAsync(ProductWisePurchaseReportFilterDto filter, CancellationToken cancellationToken = default);
}
