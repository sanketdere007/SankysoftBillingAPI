using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface ISalesEntryRepository
{
    Task<ApiResponse<SalesEntrySaveResult>> SaveSalesEntryAsync(SalesEntrySaveRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<SalesMasterListModel>>> GetAllSalesMasterAsync(SalesMasterFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<SalesDetailListModel>>> GetAllSalesDetailAsync(int salesMasterId, CancellationToken cancellationToken = default);
}
