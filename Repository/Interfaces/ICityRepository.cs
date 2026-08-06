using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

/// <summary>
/// Repository interface defining data access operations for City management using ADO.NET and Stored Procedures.
/// </summary>
public interface ICityRepository
{
    /// <summary>
    /// Executes SP_City_InsertOrUpdate stored procedure passing JSON serialized city data (@CityJsonData).
    /// Inserts new city if City_Id is 0, or updates existing city if City_Id > 0.
    /// </summary>
    /// <param name="city">City model containing details to insert or update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status and Message wrapped in ApiResponse</returns>
    Task<ApiResponse<CitySaveResult>> SaveCityAsync(CityModel city, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches city records from SQL Server using SP_City_GetAll stored procedure with optional filters.
    /// </summary>
    /// <param name="filter">Optional query filter parameters: Search, StateId, IsActive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of cities matching filter criteria wrapped in ApiResponse</returns>
    Task<ApiResponse<List<CityListModel>>> GetAllCitiesAsync(CityFilterDto? filter = null, CancellationToken cancellationToken = default);
}
