using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface ISupplierRepository
{
    Task<ApiResponse<SupplierSaveResult>> SaveSupplierAsync(SupplierModel supplier, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<SupplierListModel>>> GetAllSuppliersAsync(SupplierFilterDto? filter = null, CancellationToken cancellationToken = default);
}
