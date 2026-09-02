using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IBatchRepository
{
    Task<ApiResponse<List<BatchListModel>>> GetAllBatchesAsync(BatchFilterDto filter, CancellationToken cancellationToken = default);
}
