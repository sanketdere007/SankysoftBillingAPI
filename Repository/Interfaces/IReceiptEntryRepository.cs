using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IReceiptEntryRepository
{
    Task<ApiResponse<ReceiptEntrySaveResult>> SaveReceiptEntryAsync(ReceiptEntrySaveRequest request, CancellationToken cancellationToken = default);
}
