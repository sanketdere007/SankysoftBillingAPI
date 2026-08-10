using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IBrandRepository
{
    Task<ApiResponse<BrandSaveResult>> SaveBrandAsync(BrandModel brand, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<BrandListModel>>> GetAllBrandsAsync(BrandFilterDto? filter = null, CancellationToken cancellationToken = default);
}
