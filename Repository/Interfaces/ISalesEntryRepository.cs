using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface ISalesEntryRepository
{
    Task<ApiResponse<SalesEntrySaveResult>> SaveSalesEntryAsync(SalesEntrySaveRequest request, CancellationToken cancellationToken = default);
}
