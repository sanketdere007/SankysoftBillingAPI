using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

/// <summary>
/// Repository interface defining branch master data retrieval operations.
/// </summary>
public interface IBranchRepository
{
    /// <summary>
    /// Fetches all branch records from SQL Server via SP_Branch_GetAll stored procedure.
    /// </summary>
    /// <param name="filter">Optional filter parameters (Company ID, active status)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Standard API response containing list of branches</returns>
    Task<ApiResponse<List<BranchListModel>>> GetAllBranchesAsync(BranchFilterDto? filter = null, CancellationToken cancellationToken = default);
}
