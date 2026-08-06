using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

/// <summary>
/// ADO.NET repository implementation for City data operations using Stored Procedures.
/// </summary>
public class CityRepository : ICityRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<CityRepository> _logger;

    public CityRepository(DbHelper dbHelper, ILogger<CityRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes SP_City_InsertOrUpdate by serializing the City model to JSON and passing as @CityJsonData.
    /// Handles insert (City_Id = 0) and update (City_Id > 0).
    /// </summary>
    public async Task<ApiResponse<CitySaveResult>> SaveCityAsync(CityModel city, CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert City model into JSON string to pass into @CityJsonData parameter
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null // Preserve PascalCase property names matching SQL JSON_VALUE
            };
            var jsonData = JsonSerializer.Serialize(city, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_City_InsertOrUpdate",
                jsonParameterName: "@CityJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new CitySaveResult();
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (colName.Equals("Status", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                var statusVal = reader.GetValue(i);
                                result.Status = statusVal is bool b ? b : Convert.ToInt32(statusVal) == 1;
                            }
                            else if (colName.Equals("Message", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Message = Convert.ToString(reader.GetValue(i)) ?? string.Empty;
                            }
                            else if (colName.Equals("City_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.City_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<CitySaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<CitySaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save city record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving city record. City_Id: {CityId}, City_Name: {CityName}", city.City_Id, city.City_Name);
            return ApiResponse<CitySaveResult>.FailureResult(
                message: "A database error occurred while processing city data.",
                error: sqlEx.Message,
                data: new CitySaveResult { Status = false, Message = sqlEx.Message, City_Id = city.City_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving city record. City_Id: {CityId}", city.City_Id);
            return ApiResponse<CitySaveResult>.FailureResult(
                message: "An unexpected error occurred while saving city.",
                error: ex.Message,
                data: new CitySaveResult { Status = false, Message = "Unexpected error occurred.", City_Id = city.City_Id });
        }
    }

    /// <summary>
    /// Fetches city records from SQL Server using SP_City_GetAll stored procedure with optional filters.
    /// </summary>
    public async Task<ApiResponse<List<CityListModel>>> GetAllCitiesAsync(CityFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Search", string.IsNullOrWhiteSpace(filter?.Search) ? DBNull.Value : filter.Search.Trim(), SqlDbType.NVarChar, 100),
                DbHelper.CreateParameter("@StateId", filter?.StateId.HasValue == true ? filter.StateId.Value : DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@IsActive", filter?.IsActive.HasValue == true ? filter.IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var cities = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_City_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<CityListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapCityFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<CityListModel>>.SuccessResult(
                data: cities,
                message: $"Successfully retrieved {cities.Count} city record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching cities using SP_City_GetAll.");
            return ApiResponse<List<CityListModel>>.FailureResult(
                message: "Unable to retrieve city list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching cities.");
            return ApiResponse<List<CityListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching cities.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Helper method to safely map SqlDataReader columns to a CityListModel instance.
    /// </summary>
    public static CityListModel MapCityFromReader(SqlDataReader reader)
    {
        var model = new CityListModel();

        if (HasColumn(reader, "City_Id") && !reader.IsDBNull(reader.GetOrdinal("City_Id")))
            model.City_Id = Convert.ToInt32(reader["City_Id"]);

        if (HasColumn(reader, "City_Name") && !reader.IsDBNull(reader.GetOrdinal("City_Name")))
            model.City_Name = Convert.ToString(reader["City_Name"]) ?? string.Empty;

        if (HasColumn(reader, "State_Name") && !reader.IsDBNull(reader.GetOrdinal("State_Name")))
            model.State_Name = Convert.ToString(reader["State_Name"]) ?? string.Empty;

        if (HasColumn(reader, "State_Code") && !reader.IsDBNull(reader.GetOrdinal("State_Code")))
            model.State_Code = Convert.ToString(reader["State_Code"]) ?? string.Empty;

        if (HasColumn(reader, "City_IsActive") && !reader.IsDBNull(reader.GetOrdinal("City_IsActive")))
            model.City_IsActive = Convert.ToBoolean(reader["City_IsActive"]);

        return model;
    }

    private static bool HasColumn(SqlDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
