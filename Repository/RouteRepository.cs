using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class RouteRepository : IRouteRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<RouteRepository> _logger;

    public RouteRepository(DbHelper dbHelper, ILogger<RouteRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<RouteSaveResult>> SaveRouteAsync(RouteModel route, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null
            };
            var jsonData = JsonSerializer.Serialize(route, jsonOptions);

            var parameters = new[]
            {
                DbHelper.CreateParameter("@RouteDataJson", jsonData, SqlDbType.NVarChar, -1),
                DbHelper.CreateParameter("@Route_CreatedBy", route.Route_CreatedBy, SqlDbType.Int),
                DbHelper.CreateParameter("@Route_ModifiedBy", route.Route_ModifiedBy, SqlDbType.Int)
            };

            var saveResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Route_InsertOrUpdate",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new RouteSaveResult();
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
                            else if (colName.Equals("Route_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Route_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<RouteSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<RouteSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save route record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving route record. Route_Id: {RouteId}", route.Route_Id);
            return ApiResponse<RouteSaveResult>.FailureResult(
                message: "A database error occurred while processing route data.",
                error: sqlEx.Message,
                data: new RouteSaveResult { Status = false, Message = sqlEx.Message, Route_Id = route.Route_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving route record. Route_Id: {RouteId}", route.Route_Id);
            return ApiResponse<RouteSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving route.",
                error: ex.Message,
                data: new RouteSaveResult { Status = false, Message = "Unexpected error occurred.", Route_Id = route.Route_Id });
        }
    }

    public async Task<ApiResponse<List<RouteListModel>>> GetAllRoutesAsync(RouteFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Route_CompId", filter?.Route_CompId ?? (object)DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@Route_BranchId", filter?.Route_BranchId ?? (object)DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@Search", filter?.Search ?? (object)DBNull.Value, SqlDbType.NVarChar, 150),
                DbHelper.CreateParameter("@IsActive", filter?.IsActive.HasValue == true ? (object)filter.IsActive.Value : DBNull.Value, SqlDbType.Bit),
                DbHelper.CreateParameter("@PageNumber", filter?.PageNumber ?? 1, SqlDbType.Int),
                DbHelper.CreateParameter("@PageSize", filter?.PageSize ?? 20, SqlDbType.Int)
            };

            var routes = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Route_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<RouteListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapRouteFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<RouteListModel>>.SuccessResult(
                data: routes,
                message: $"Successfully retrieved {routes.Count} route record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching routes.");
            return ApiResponse<List<RouteListModel>>.FailureResult(
                message: "Unable to retrieve route list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching routes.");
            return ApiResponse<List<RouteListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching routes.",
                error: ex.Message);
        }
    }

    private static RouteListModel MapRouteFromReader(SqlDataReader reader)
    {
        var model = new RouteListModel();

        if (HasColumn(reader, "Route_Id") && !reader.IsDBNull(reader.GetOrdinal("Route_Id")))
            model.Route_Id = Convert.ToInt32(reader["Route_Id"]);

        if (HasColumn(reader, "Route_CompId") && !reader.IsDBNull(reader.GetOrdinal("Route_CompId")))
            model.Route_CompId = Convert.ToInt32(reader["Route_CompId"]);

        if (HasColumn(reader, "Route_BranchId") && !reader.IsDBNull(reader.GetOrdinal("Route_BranchId")))
            model.Route_BranchId = Convert.ToInt32(reader["Route_BranchId"]);

        if (HasColumn(reader, "Route_Name") && !reader.IsDBNull(reader.GetOrdinal("Route_Name")))
            model.Route_Name = Convert.ToString(reader["Route_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Route_Description") && !reader.IsDBNull(reader.GetOrdinal("Route_Description")))
            model.Route_Description = Convert.ToString(reader["Route_Description"]);

        if (HasColumn(reader, "Route_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Route_IsActive")))
            model.Route_IsActive = Convert.ToBoolean(reader["Route_IsActive"]);

        if (HasColumn(reader, "Route_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("Route_CreatedBy")))
            model.Route_CreatedBy = Convert.ToInt32(reader["Route_CreatedBy"]);

        if (HasColumn(reader, "Route_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Route_CreatedDate")))
            model.Route_CreatedDate = Convert.ToDateTime(reader["Route_CreatedDate"]);

        if (HasColumn(reader, "Route_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("Route_ModifiedBy")))
            model.Route_ModifiedBy = Convert.ToInt32(reader["Route_ModifiedBy"]);

        if (HasColumn(reader, "Route_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Route_ModifiedDate")))
            model.Route_ModifiedDate = Convert.ToDateTime(reader["Route_ModifiedDate"]);

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
