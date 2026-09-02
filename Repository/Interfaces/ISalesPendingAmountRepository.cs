using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface ISalesPendingAmountRepository
{
    Task<ApiResponse<PagedListResult<SalesPendingAmountModel>>> GetPendingAmountAsync(
        SalesPendingAmountFilterDto? filter = null,
        CancellationToken cancellationToken = default);
}
