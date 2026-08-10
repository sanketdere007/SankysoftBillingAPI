using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

public interface ISubCategoryRepository
{
    Task<ApiResponse<SubCategorySaveResult>> SaveSubCategoryAsync(SubCategoryModel subCategory, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<SubCategoryListModel>>> GetAllSubCategoriesAsync(SubCategoryFilterDto? filter = null, CancellationToken cancellationToken = default);
}
