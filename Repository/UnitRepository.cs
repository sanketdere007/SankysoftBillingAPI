using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class UnitRepository : IUnitRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<UnitRepository> _logger;

    public UnitRepository(DbHelper dbHelper, ILogger<UnitRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<UnitSaveResult>> SaveUnitAsync(UnitModel unit, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null
            };
            var jsonData = JsonSerializer.Serialize(unit, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_Unit_InsertOrUpdate",
                jsonParameterName: "@UnitJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new UnitSaveResult();
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
                            else if (colName.Equals("Unit_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Unit_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<UnitSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<UnitSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save unit record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving unit record. Unit_Id: {UnitId}", unit.Unit_Id);
            return ApiResponse<UnitSaveResult>.FailureResult(
                message: "A database error occurred while processing unit data.",
                error: sqlEx.Message,
                data: new UnitSaveResult { Status = false, Message = sqlEx.Message, Unit_Id = unit.Unit_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving unit record. Unit_Id: {UnitId}", unit.Unit_Id);
            return ApiResponse<UnitSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving unit.",
                error: ex.Message,
                data: new UnitSaveResult { Status = false, Message = "Unexpected error occurred.", Unit_Id = unit.Unit_Id });
        }
    }

    public async Task<ApiResponse<List<UnitListModel>>> GetAllUnitsAsync(UnitFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Unit_Id", filter?.Unit_Id ?? (object)DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@Unit_IsActive", filter?.Unit_IsActive.HasValue == true ? (object)filter.Unit_IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var units = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Unit_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<UnitListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapUnitFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<UnitListModel>>.SuccessResult(
                data: units,
                message: $"Successfully retrieved {units.Count} unit record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching units.");
            return ApiResponse<List<UnitListModel>>.FailureResult(
                message: "Unable to retrieve unit list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching units.");
            return ApiResponse<List<UnitListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching units.",
                error: ex.Message);
        }
    }

    private static UnitListModel MapUnitFromReader(SqlDataReader reader)
    {
        var model = new UnitListModel();

        if (HasColumn(reader, "Unit_Id") && !reader.IsDBNull(reader.GetOrdinal("Unit_Id")))
            model.Unit_Id = Convert.ToInt32(reader["Unit_Id"]);

        if (HasColumn(reader, "Unit_Name") && !reader.IsDBNull(reader.GetOrdinal("Unit_Name")))
            model.Unit_Name = Convert.ToString(reader["Unit_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Unit_ShortName") && !reader.IsDBNull(reader.GetOrdinal("Unit_ShortName")))
            model.Unit_ShortName = Convert.ToString(reader["Unit_ShortName"]) ?? string.Empty;

        if (HasColumn(reader, "Unit_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Unit_IsActive")))
            model.Unit_IsActive = Convert.ToBoolean(reader["Unit_IsActive"]);

        if (HasColumn(reader, "Unit_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("Unit_CreatedBy")))
            model.Unit_CreatedBy = Convert.ToInt32(reader["Unit_CreatedBy"]);

        if (HasColumn(reader, "Unit_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Unit_CreatedDate")))
            model.Unit_CreatedDate = Convert.ToDateTime(reader["Unit_CreatedDate"]);

        if (HasColumn(reader, "Unit_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("Unit_ModifiedBy")))
            model.Unit_ModifiedBy = Convert.ToInt32(reader["Unit_ModifiedBy"]);

        if (HasColumn(reader, "Unit_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Unit_ModifiedDate")))
            model.Unit_ModifiedDate = Convert.ToDateTime(reader["Unit_ModifiedDate"]);

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
