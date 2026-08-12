using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface IProductRepository
{
    Task<ApiResponse<ProductSaveResult>> SaveProductAsync(ProductModel product, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<ProductListModel>>> GetAllProductsAsync(ProductFilterDto filter, CancellationToken cancellationToken = default);
}
