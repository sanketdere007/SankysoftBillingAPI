using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IPurchaseEntryRepository
{
    Task<ApiResponse<PurchaseEntrySaveResult>> SavePurchaseEntryAsync(PurchaseEntrySaveRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedListResult<PurchaseMasterViewModel>>> GetPurchaseMasterViewListAsync(PurchaseMasterViewListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<PurchaseDetailViewModel>>> GetPurchaseDetailViewListAsync(int purchaseMasterId, CancellationToken cancellationToken = default);
}
