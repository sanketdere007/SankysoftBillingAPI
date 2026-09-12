using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IReportRepository
{
    Task<ApiResponse<PagedListResult<ProductWiseSalesReportModel>>> GetProductWiseSalesReportAsync(ProductWiseSalesReportFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<ProductWiseCustomerPurchaseListModel>>> GetProductWiseCustomerPurchaseListAsync(ProductWiseCustomerPurchaseListFilterDto filter, CancellationToken cancellationToken = default);
}
