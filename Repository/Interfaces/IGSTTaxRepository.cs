using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IGSTTaxRepository
{
    Task<ApiResponse<GSTTaxSaveResult>> SaveGSTTaxAsync(GSTTaxModel gstTax, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<GSTTaxListModel>>> GetAllGSTTaxesAsync(GSTTaxFilterDto? filter = null, CancellationToken cancellationToken = default);
}
