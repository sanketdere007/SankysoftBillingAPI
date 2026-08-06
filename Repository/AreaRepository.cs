using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

/// <summary>
/// ADO.NET repository implementation for Area data operations using Stored Procedures.
/// </summary>
public class AreaRepository : IAreaRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<AreaRepository> _logger;

    public AreaRepository(DbHelper dbHelper, ILogger<AreaRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes SP_Area_InsertOrUpdate by serializing the Area model to JSON and passing as @AreaJsonData.
    /// Handles insert (Area_Id = 0) and update (Area_Id > 0).
    /// </summary>
    public async Task<ApiResponse<AreaSaveResult>> SaveAreaAsync(AreaModel area, CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert Area model into JSON string to pass into @AreaJsonData parameter
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null // Preserve PascalCase property names matching SQL JSON_VALUE
            };
            var jsonData = JsonSerializer.Serialize(area, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_Area_InsertOrUpdate",
                jsonParameterName: "@AreaJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new AreaSaveResult();
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
                            else if (colName.Equals("Area_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Area_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<AreaSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<AreaSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save area record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving area record. Area_Id: {AreaId}, Area_Name: {AreaName}", area.Area_Id, area.Area_Name);
            return ApiResponse<AreaSaveResult>.FailureResult(
                message: "A database error occurred while processing area data.",
                error: sqlEx.Message,
                data: new AreaSaveResult { Status = false, Message = sqlEx.Message, Area_Id = area.Area_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving area record. Area_Id: {AreaId}", area.Area_Id);
            return ApiResponse<AreaSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving area.",
                error: ex.Message,
                data: new AreaSaveResult { Status = false, Message = "Unexpected error occurred.", Area_Id = area.Area_Id });
        }
    }

    /// <summary>
    /// Fetches area records from SQL Server using SP_Area_GetAll stored procedure with optional filters.
    /// </summary>
    public async Task<ApiResponse<List<AreaListModel>>> GetAllAreasAsync(AreaFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Search", string.IsNullOrWhiteSpace(filter?.Search) ? DBNull.Value : filter.Search.Trim(), SqlDbType.NVarChar, 100),
                DbHelper.CreateParameter("@StateId", filter?.StateId.HasValue == true ? filter.StateId.Value : DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@CityId", filter?.CityId.HasValue == true ? filter.CityId.Value : DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@Pincode", string.IsNullOrWhiteSpace(filter?.Pincode) ? DBNull.Value : filter.Pincode.Trim(), SqlDbType.NVarChar, 10),
                DbHelper.CreateParameter("@IsActive", filter?.IsActive.HasValue == true ? filter.IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var areas = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Area_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<AreaListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapAreaFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<AreaListModel>>.SuccessResult(
                data: areas,
                message: $"Successfully retrieved {areas.Count} area record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching areas using SP_Area_GetAll.");
            return ApiResponse<List<AreaListModel>>.FailureResult(
                message: "Unable to retrieve area list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching areas.");
            return ApiResponse<List<AreaListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching areas.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Helper method to safely map SqlDataReader columns to an AreaListModel instance.
    /// </summary>
    public static AreaListModel MapAreaFromReader(SqlDataReader reader)
    {
        var model = new AreaListModel();

        if (HasColumn(reader, "Area_Id") && !reader.IsDBNull(reader.GetOrdinal("Area_Id")))
            model.Area_Id = Convert.ToInt32(reader["Area_Id"]);

        if (HasColumn(reader, "Area_Name") && !reader.IsDBNull(reader.GetOrdinal("Area_Name")))
            model.Area_Name = Convert.ToString(reader["Area_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Area_Pincode") && !reader.IsDBNull(reader.GetOrdinal("Area_Pincode")))
            model.Area_Pincode = Convert.ToString(reader["Area_Pincode"]) ?? string.Empty;

        if (HasColumn(reader, "City_Name") && !reader.IsDBNull(reader.GetOrdinal("City_Name")))
            model.City_Name = Convert.ToString(reader["City_Name"]) ?? string.Empty;

        if (HasColumn(reader, "State_Name") && !reader.IsDBNull(reader.GetOrdinal("State_Name")))
            model.State_Name = Convert.ToString(reader["State_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Area_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Area_IsActive")))
            model.Area_IsActive = Convert.ToBoolean(reader["Area_IsActive"]);

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
