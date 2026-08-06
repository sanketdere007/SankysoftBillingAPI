using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

/// <summary>
/// Repository interface defining data access operations for State master data using ADO.NET and Stored Procedures.
/// </summary>
public interface IStateRepository
{
    /// <summary>
    /// Fetches state records from SQL Server using SP_State_GetAll stored procedure with optional filters.
    /// </summary>
    /// <param name="filter">Optional query filter parameters: Search, IsActive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of states matching filter criteria wrapped in ApiResponse</returns>
    Task<ApiResponse<List<StateListModel>>> GetAllStatesAsync(StateFilterDto? filter = null, CancellationToken cancellationToken = default);
}
