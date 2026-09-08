using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IBatchRepository
{
    Task<ApiResponse<List<BatchListModel>>> GetAllBatchesAsync(BatchFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ProductStockModel>>> GetAllProductStockAsync(ProductStockFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<BatchSaveResult>> SaveBatchAsync(BatchSaveModel batch, CancellationToken cancellationToken = default);
}
