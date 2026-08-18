using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IPurchaseEntryRepository
{
    Task<ApiResponse<PurchaseEntrySaveResult>> SavePurchaseEntryAsync(PurchaseEntrySaveRequest request, CancellationToken cancellationToken = default);
}
