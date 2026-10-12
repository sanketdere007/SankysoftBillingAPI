using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IPurchaseReturnEntryRepository
{
    Task<ApiResponse<PurchaseReturnEntrySaveResult>> SavePurchaseReturnEntryAsync(PurchaseReturnEntrySaveRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<PurchaseReturnMasterViewListResponse>>> GetPurchaseReturnMasterViewListAsync(PurchaseReturnMasterViewListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PurchaseReturnDetailViewListResponse>>> GetPurchaseReturnDetailViewListAsync(PurchaseReturnDetailViewListRequest request, CancellationToken cancellationToken = default);
}
