using System.Data;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class BatchRepository : IBatchRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<BatchRepository> _logger;

    public BatchRepository(DbHelper dbHelper, ILogger<BatchRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<List<BatchListModel>>> GetAllBatchesAsync(BatchFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@CompId", filter.CompId, SqlDbType.Int),
                DbHelper.CreateParameter("@BranchId", filter.BranchId, SqlDbType.Int),
                DbHelper.CreateParameter("@ProductId", filter.ProductId, SqlDbType.Int),
                DbHelper.CreateParameter("@Search", filter.Search, SqlDbType.NVarChar)
            };

            var batches = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Batch_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<BatchListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapBatchFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<BatchListModel>>.SuccessResult(
                data: batches,
                message: $"Successfully retrieved {batches.Count} batch record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching batches.");
            return ApiResponse<List<BatchListModel>>.FailureResult(
                message: "Unable to retrieve batch list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching batches.");
            return ApiResponse<List<BatchListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching batches.",
                error: ex.Message);
        }
    }

    private static BatchListModel MapBatchFromReader(SqlDataReader reader)
    {
        var model = new BatchListModel();

        if (HasColumn(reader, "Batch_Id") && !reader.IsDBNull(reader.GetOrdinal("Batch_Id")))
            model.Batch_Id = Convert.ToInt32(reader["Batch_Id"]);

        if (HasColumn(reader, "Batch_ProductId") && !reader.IsDBNull(reader.GetOrdinal("Batch_ProductId")))
            model.Batch_ProductId = Convert.ToInt32(reader["Batch_ProductId"]);

        if (HasColumn(reader, "Prod_Name") && !reader.IsDBNull(reader.GetOrdinal("Prod_Name")))
            model.Prod_Name = Convert.ToString(reader["Prod_Name"]);

        if (HasColumn(reader, "Prod_Code") && !reader.IsDBNull(reader.GetOrdinal("Prod_Code")))
            model.Prod_Code = Convert.ToString(reader["Prod_Code"]);

        if (HasColumn(reader, "Unit_Name") && !reader.IsDBNull(reader.GetOrdinal("Unit_Name")))
            model.Unit_Name = Convert.ToString(reader["Unit_Name"]);

        if (HasColumn(reader, "Prod_UnitValue") && !reader.IsDBNull(reader.GetOrdinal("Prod_UnitValue")))
            model.Prod_UnitValue = Convert.ToDecimal(reader["Prod_UnitValue"]);

        if (HasColumn(reader, "Batch_CompId") && !reader.IsDBNull(reader.GetOrdinal("Batch_CompId")))
            model.Batch_CompId = Convert.ToInt32(reader["Batch_CompId"]);

        if (HasColumn(reader, "Comp_Name") && !reader.IsDBNull(reader.GetOrdinal("Comp_Name")))
            model.Comp_Name = Convert.ToString(reader["Comp_Name"]);

        if (HasColumn(reader, "Batch_BranchId") && !reader.IsDBNull(reader.GetOrdinal("Batch_BranchId")))
            model.Batch_BranchId = Convert.ToInt32(reader["Batch_BranchId"]);

        if (HasColumn(reader, "Branch_Name") && !reader.IsDBNull(reader.GetOrdinal("Branch_Name")))
            model.Branch_Name = Convert.ToString(reader["Branch_Name"]);

        if (HasColumn(reader, "Batch_Stock") && !reader.IsDBNull(reader.GetOrdinal("Batch_Stock")))
            model.Batch_Stock = Convert.ToDecimal(reader["Batch_Stock"]);

        if (HasColumn(reader, "Batch_AvailableStock") && !reader.IsDBNull(reader.GetOrdinal("Batch_AvailableStock")))
            model.Batch_AvailableStock = Convert.ToDecimal(reader["Batch_AvailableStock"]);

        if (HasColumn(reader, "Batch_LandingPrice") && !reader.IsDBNull(reader.GetOrdinal("Batch_LandingPrice")))
            model.Batch_LandingPrice = Convert.ToDecimal(reader["Batch_LandingPrice"]);

        if (HasColumn(reader, "Batch_PurchasePrice") && !reader.IsDBNull(reader.GetOrdinal("Batch_PurchasePrice")))
            model.Batch_PurchasePrice = Convert.ToDecimal(reader["Batch_PurchasePrice"]);

        if (HasColumn(reader, "Batch_MRP") && !reader.IsDBNull(reader.GetOrdinal("Batch_MRP")))
            model.Batch_MRP = Convert.ToDecimal(reader["Batch_MRP"]);

        if (HasColumn(reader, "Batch_SellingPrice") && !reader.IsDBNull(reader.GetOrdinal("Batch_SellingPrice")))
            model.Batch_SellingPrice = Convert.ToDecimal(reader["Batch_SellingPrice"]);

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
