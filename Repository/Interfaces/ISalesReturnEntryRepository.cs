using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface ISalesReturnEntryRepository
{
    Task<ApiResponse<SalesReturnEntrySaveResult>> SaveSalesReturnEntryAsync(SalesReturnEntrySaveRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<SalesReturnEntryDeleteResult>> DeleteSalesReturnEntryAsync(int salesReturnMasterId, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<SalesReturnListResponse>>> GetSalesReturnListAsync(SalesReturnListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<SalesReturnEntryDetailResponse>>> GetSalesReturnEntryDetailListAsync(int salesReturnMasterId, CancellationToken cancellationToken = default);
}
