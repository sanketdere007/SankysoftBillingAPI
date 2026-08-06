using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

/// <summary>
/// Repository interface defining data access operations for Area management using ADO.NET and Stored Procedures.
/// </summary>
public interface IAreaRepository
{
    /// <summary>
    /// Executes SP_Area_InsertOrUpdate stored procedure passing JSON serialized area data (@AreaJsonData).
    /// Inserts new area if Area_Id is 0, or updates existing area if Area_Id > 0.
    /// </summary>
    /// <param name="area">Area model containing details to insert or update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status and Message wrapped in ApiResponse</returns>
    Task<ApiResponse<AreaSaveResult>> SaveAreaAsync(AreaModel area, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches area records from SQL Server using SP_Area_GetAll stored procedure with optional filters.
    /// </summary>
    /// <param name="filter">Optional query filter parameters: Search, StateId, CityId, Pincode, IsActive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of areas matching filter criteria wrapped in ApiResponse</returns>
    Task<ApiResponse<List<AreaListModel>>> GetAllAreasAsync(AreaFilterDto? filter = null, CancellationToken cancellationToken = default);
}
