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

    public async Task<ApiResponse<List<ProductStockModel>>> GetAllProductStockAsync(ProductStockFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@CompId", filter.CompId, SqlDbType.Int),
                DbHelper.CreateParameter("@BranchId", filter.BranchId, SqlDbType.Int),
                DbHelper.CreateParameter("@ProductId", filter.ProductId, SqlDbType.Int),
                DbHelper.CreateParameter("@Search", filter.Search, SqlDbType.NVarChar),
                DbHelper.CreateParameter("@IsActive", filter.IsActive, SqlDbType.Bit),
                DbHelper.CreateParameter("@PageNumber", filter.PageNumber, SqlDbType.Int),
                DbHelper.CreateParameter("@PageSize", filter.PageSize, SqlDbType.Int)
            };

            var stocks = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Batch_GetAllProductStock",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<ProductStockModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapProductStockFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<ProductStockModel>>.SuccessResult(
                data: stocks,
                message: $"Successfully retrieved {stocks.Count} product stock record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching product stock.");
            return ApiResponse<List<ProductStockModel>>.FailureResult(
                message: "Unable to retrieve product stock list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching product stock.");
            return ApiResponse<List<ProductStockModel>>.FailureResult(
                message: "An unexpected error occurred while fetching product stock.",
                error: ex.Message);
        }
    }

    private static ProductStockModel MapProductStockFromReader(SqlDataReader reader)
    {
        var model = new ProductStockModel();

        if (HasColumn(reader, "Batch_CompId") && !reader.IsDBNull(reader.GetOrdinal("Batch_CompId")))
            model.Batch_CompId = Convert.ToInt32(reader["Batch_CompId"]);

        if (HasColumn(reader, "Batch_BranchId") && !reader.IsDBNull(reader.GetOrdinal("Batch_BranchId")))
            model.Batch_BranchId = Convert.ToInt32(reader["Batch_BranchId"]);

        if (HasColumn(reader, "Batch_ProductId") && !reader.IsDBNull(reader.GetOrdinal("Batch_ProductId")))
            model.Batch_ProductId = Convert.ToInt32(reader["Batch_ProductId"]);

        if (HasColumn(reader, "ProductName") && !reader.IsDBNull(reader.GetOrdinal("ProductName")))
            model.ProductName = Convert.ToString(reader["ProductName"]);
        else if (HasColumn(reader, "Prod_Name") && !reader.IsDBNull(reader.GetOrdinal("Prod_Name")))
            model.ProductName = Convert.ToString(reader["Prod_Name"]);

        if (HasColumn(reader, "Prod_UnitId") && !reader.IsDBNull(reader.GetOrdinal("Prod_UnitId")))
            model.Prod_UnitId = Convert.ToInt32(reader["Prod_UnitId"]);

        if (HasColumn(reader, "Prod_UnitValue") && !reader.IsDBNull(reader.GetOrdinal("Prod_UnitValue")))
            model.Prod_UnitValue = Convert.ToDecimal(reader["Prod_UnitValue"]);

        if (HasColumn(reader, "UnitName") && !reader.IsDBNull(reader.GetOrdinal("UnitName")))
            model.UnitName = Convert.ToString(reader["UnitName"]);

        if (HasColumn(reader, "UnitShortName") && !reader.IsDBNull(reader.GetOrdinal("UnitShortName")))
            model.UnitShortName = Convert.ToString(reader["UnitShortName"]);

        if (HasColumn(reader, "Prod_BrandId") && !reader.IsDBNull(reader.GetOrdinal("Prod_BrandId")))
            model.Prod_BrandId = Convert.ToInt32(reader["Prod_BrandId"]);

        if (HasColumn(reader, "BrandName") && !reader.IsDBNull(reader.GetOrdinal("BrandName")))
            model.BrandName = Convert.ToString(reader["BrandName"]);

        if (HasColumn(reader, "Prod_CategoryId") && !reader.IsDBNull(reader.GetOrdinal("Prod_CategoryId")))
            model.Prod_CategoryId = Convert.ToInt32(reader["Prod_CategoryId"]);

        if (HasColumn(reader, "CategoryName") && !reader.IsDBNull(reader.GetOrdinal("CategoryName")))
            model.CategoryName = Convert.ToString(reader["CategoryName"]);

        if (HasColumn(reader, "Prod_SubCategoryId") && !reader.IsDBNull(reader.GetOrdinal("Prod_SubCategoryId")))
            model.Prod_SubCategoryId = Convert.ToInt32(reader["Prod_SubCategoryId"]);

        if (HasColumn(reader, "SubCategoryName") && !reader.IsDBNull(reader.GetOrdinal("SubCategoryName")))
            model.SubCategoryName = Convert.ToString(reader["SubCategoryName"]);

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

        if (HasColumn(reader, "Batch_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Batch_IsActive")))
            model.Batch_IsActive = Convert.ToInt32(reader["Batch_IsActive"]);

        if (HasColumn(reader, "Batch_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Batch_CreatedDate")))
            model.Batch_CreatedDate = Convert.ToDateTime(reader["Batch_CreatedDate"]);

        if (HasColumn(reader, "Batch_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Batch_ModifiedDate")))
            model.Batch_ModifiedDate = Convert.ToDateTime(reader["Batch_ModifiedDate"]);

        return model;
    }

    public async Task<ApiResponse<BatchSaveResult>> SaveBatchAsync(BatchSaveModel batch, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null
            };
            var jsonData = System.Text.Json.JsonSerializer.Serialize(batch, jsonOptions);

            var parameters = new[]
            {
                DbHelper.CreateParameter("@BatchDataJson", jsonData, SqlDbType.NVarChar, -1),
                DbHelper.CreateParameter("@Batch_CreatedBy", batch.Batch_CreatedBy, SqlDbType.Int),
                DbHelper.CreateParameter("@Batch_ModifiedBy", batch.Batch_ModifiedBy, SqlDbType.Int)
            };

            var saveResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Batch_InsertOrUpdate",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new BatchSaveResult();
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
                            else if (colName.Equals("Batch_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Batch_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                            else if (colName.Equals("OldStock", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.OldStock = Convert.ToDecimal(reader.GetValue(i));
                            }
                            else if (colName.Equals("OldAvailableStock", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.OldAvailableStock = Convert.ToDecimal(reader.GetValue(i));
                            }
                            else if (colName.Equals("NewStock", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.NewStock = Convert.ToDecimal(reader.GetValue(i));
                            }
                            else if (colName.Equals("NewAvailableStock", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.NewAvailableStock = Convert.ToDecimal(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<BatchSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<BatchSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save batch record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving batch record. Batch_Id: {BatchId}", batch.Batch_Id);
            return ApiResponse<BatchSaveResult>.FailureResult(
                message: "A database error occurred while processing batch data.",
                error: sqlEx.Message,
                data: new BatchSaveResult { Status = false, Message = sqlEx.Message, Batch_Id = batch.Batch_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving batch record. Batch_Id: {BatchId}", batch.Batch_Id);
            return ApiResponse<BatchSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving batch.",
                error: ex.Message,
                data: new BatchSaveResult { Status = false, Message = "Unexpected error occurred.", Batch_Id = batch.Batch_Id });
        }
    }
}
