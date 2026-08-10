using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface ICategoryRepository
{
    Task<ApiResponse<CategorySaveResult>> SaveCategoryAsync(CategoryModel category, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<CategoryListModel>>> GetAllCategoriesAsync(CategoryFilterDto? filter = null, CancellationToken cancellationToken = default);
}
