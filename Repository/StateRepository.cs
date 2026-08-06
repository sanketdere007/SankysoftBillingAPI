using System.Data;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

/// <summary>
/// ADO.NET repository implementation for State data operations using Stored Procedures.
/// </summary>
public class StateRepository : IStateRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<StateRepository> _logger;

    public StateRepository(DbHelper dbHelper, ILogger<StateRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Fetches state records from SQL Server using SP_State_GetAll stored procedure with optional filters.
    /// </summary>
    public async Task<ApiResponse<List<StateListModel>>> GetAllStatesAsync(StateFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Search", string.IsNullOrWhiteSpace(filter?.Search) ? DBNull.Value : filter.Search.Trim(), SqlDbType.NVarChar, 100),
                DbHelper.CreateParameter("@IsActive", filter?.IsActive.HasValue == true ? filter.IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var states = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_State_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<StateListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapStateFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<StateListModel>>.SuccessResult(
                data: states,
                message: $"Successfully retrieved {states.Count} state record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching states using SP_State_GetAll.");
            return ApiResponse<List<StateListModel>>.FailureResult(
                message: "Unable to retrieve state list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching states.");
            return ApiResponse<List<StateListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching states.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Helper method to safely map SqlDataReader columns to a StateListModel instance.
    /// </summary>
    public static StateListModel MapStateFromReader(SqlDataReader reader)
    {
        var model = new StateListModel();

        if (HasColumn(reader, "State_Id") && !reader.IsDBNull(reader.GetOrdinal("State_Id")))
            model.State_Id = Convert.ToInt32(reader["State_Id"]);

        if (HasColumn(reader, "State_Name") && !reader.IsDBNull(reader.GetOrdinal("State_Name")))
            model.State_Name = Convert.ToString(reader["State_Name"]) ?? string.Empty;

        if (HasColumn(reader, "State_Code") && !reader.IsDBNull(reader.GetOrdinal("State_Code")))
            model.State_Code = Convert.ToString(reader["State_Code"]) ?? string.Empty;

        if (HasColumn(reader, "State_IsActive") && !reader.IsDBNull(reader.GetOrdinal("State_IsActive")))
            model.State_IsActive = Convert.ToBoolean(reader["State_IsActive"]);

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
