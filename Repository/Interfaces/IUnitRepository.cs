using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IUnitRepository
{
    Task<ApiResponse<UnitSaveResult>> SaveUnitAsync(UnitModel unit, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<UnitListModel>>> GetAllUnitsAsync(UnitFilterDto? filter = null, CancellationToken cancellationToken = default);
}
