using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IPurchaseReturnEntryRepository
{
    Task<ApiResponse<PurchaseReturnEntrySaveResult>> SavePurchaseReturnEntryAsync(PurchaseReturnEntrySaveRequest request, CancellationToken cancellationToken = default);
}
