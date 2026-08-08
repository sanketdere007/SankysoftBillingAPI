using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

/// <summary>
/// Repository interface defining company master data retrieval operations.
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Fetches all company records from SQL Server via SP_Company_GetAll stored procedure.
    /// </summary>
    /// <param name="filter">Optional filter parameters (e.g. active status)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Standard API response containing list of companies</returns>
    Task<ApiResponse<List<CompanyListModel>>> GetAllCompaniesAsync(CompanyFilterDto? filter = null, CancellationToken cancellationToken = default);
}
