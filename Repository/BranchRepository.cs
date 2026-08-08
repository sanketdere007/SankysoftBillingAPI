using System.Data;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

/// <summary>
/// ADO.NET repository implementation for Branch data operations using Stored Procedures.
/// </summary>
public class BranchRepository : IBranchRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<BranchRepository> _logger;

    public BranchRepository(DbHelper dbHelper, ILogger<BranchRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Fetches branch records from SQL Server using SP_Branch_GetAll stored procedure with optional filters.
    /// </summary>
    public async Task<ApiResponse<List<BranchListModel>>> GetAllBranchesAsync(BranchFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Branch_CompId", filter?.Branch_CompId.HasValue == true ? filter.Branch_CompId.Value : DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@Branch_IsActive", filter?.Branch_IsActive.HasValue == true ? filter.Branch_IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var branches = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Branch_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<BranchListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapBranchFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<BranchListModel>>.SuccessResult(
                data: branches,
                message: $"Successfully retrieved {branches.Count} branch record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching branches using SP_Branch_GetAll.");
            return ApiResponse<List<BranchListModel>>.FailureResult(
                message: "Unable to retrieve branch list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching branches.");
            return ApiResponse<List<BranchListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching branches.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Helper method to safely map SqlDataReader columns to a BranchListModel instance.
    /// </summary>
    public static BranchListModel MapBranchFromReader(SqlDataReader reader)
    {
        var model = new BranchListModel();

        if (HasColumn(reader, "Branch_Id") && !reader.IsDBNull(reader.GetOrdinal("Branch_Id")))
            model.Branch_Id = Convert.ToInt32(reader["Branch_Id"]);

        if (HasColumn(reader, "Branch_CompId") && !reader.IsDBNull(reader.GetOrdinal("Branch_CompId")))
            model.Branch_CompId = Convert.ToInt32(reader["Branch_CompId"]);

        if (HasColumn(reader, "Branch_CompName") && !reader.IsDBNull(reader.GetOrdinal("Branch_CompName")))
            model.Branch_CompName = Convert.ToString(reader["Branch_CompName"]) ?? string.Empty;

        if (HasColumn(reader, "Branch_Name") && !reader.IsDBNull(reader.GetOrdinal("Branch_Name")))
            model.Branch_Name = Convert.ToString(reader["Branch_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Branch_ContactPerson") && !reader.IsDBNull(reader.GetOrdinal("Branch_ContactPerson")))
            model.Branch_ContactPerson = Convert.ToString(reader["Branch_ContactPerson"]);

        if (HasColumn(reader, "Branch_MobileNo") && !reader.IsDBNull(reader.GetOrdinal("Branch_MobileNo")))
            model.Branch_MobileNo = Convert.ToString(reader["Branch_MobileNo"]);

        if (HasColumn(reader, "Branch_AlternateMobileNo") && !reader.IsDBNull(reader.GetOrdinal("Branch_AlternateMobileNo")))
            model.Branch_AlternateMobileNo = Convert.ToString(reader["Branch_AlternateMobileNo"]);

        if (HasColumn(reader, "Branch_Email") && !reader.IsDBNull(reader.GetOrdinal("Branch_Email")))
            model.Branch_Email = Convert.ToString(reader["Branch_Email"]);

        if (HasColumn(reader, "Branch_GSTNo") && !reader.IsDBNull(reader.GetOrdinal("Branch_GSTNo")))
            model.Branch_GSTNo = Convert.ToString(reader["Branch_GSTNo"]);

        if (HasColumn(reader, "Branch_Address") && !reader.IsDBNull(reader.GetOrdinal("Branch_Address")))
            model.Branch_Address = Convert.ToString(reader["Branch_Address"]);

        if (HasColumn(reader, "Branch_Area") && !reader.IsDBNull(reader.GetOrdinal("Branch_Area")))
            model.Branch_Area = Convert.ToString(reader["Branch_Area"]);

        if (HasColumn(reader, "Branch_City") && !reader.IsDBNull(reader.GetOrdinal("Branch_City")))
            model.Branch_City = Convert.ToString(reader["Branch_City"]);

        if (HasColumn(reader, "Branch_State") && !reader.IsDBNull(reader.GetOrdinal("Branch_State")))
            model.Branch_State = Convert.ToString(reader["Branch_State"]);

        if (HasColumn(reader, "Branch_Pincode") && !reader.IsDBNull(reader.GetOrdinal("Branch_Pincode")))
            model.Branch_Pincode = Convert.ToString(reader["Branch_Pincode"]);

        if (HasColumn(reader, "Branch_Country") && !reader.IsDBNull(reader.GetOrdinal("Branch_Country")))
            model.Branch_Country = Convert.ToString(reader["Branch_Country"]);

        if (HasColumn(reader, "Branch_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Branch_IsActive")))
            model.Branch_IsActive = Convert.ToBoolean(reader["Branch_IsActive"]);

        if (HasColumn(reader, "Branch_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("Branch_CreatedBy")))
            model.Branch_CreatedBy = Convert.ToInt32(reader["Branch_CreatedBy"]);

        if (HasColumn(reader, "Branch_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Branch_CreatedDate")))
            model.Branch_CreatedDate = Convert.ToDateTime(reader["Branch_CreatedDate"]);

        if (HasColumn(reader, "Branch_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("Branch_ModifiedBy")))
            model.Branch_ModifiedBy = Convert.ToInt32(reader["Branch_ModifiedBy"]);

        if (HasColumn(reader, "Branch_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Branch_ModifiedDate")))
            model.Branch_ModifiedDate = Convert.ToDateTime(reader["Branch_ModifiedDate"]);

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
