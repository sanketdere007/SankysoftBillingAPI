using System.Data;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using System.Text.Json;
namespace Billing_Software_Api.Repository;

public class ProductRepository : IProductRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(DbHelper dbHelper, ILogger<ProductRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<ProductSaveResult>> SaveProductAsync(ProductModel product, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null
            };
            string jsonData = JsonSerializer.Serialize(product, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_Product_InsertOrUpdate",
                jsonParameterName: "@ProdJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new ProductSaveResult();
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
                            else if (colName.Equals("Prod_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Prod_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<ProductSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<ProductSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save product record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving product record. Prod_Id: {ProdId}", product.Prod_Id);
            return ApiResponse<ProductSaveResult>.FailureResult(
                message: "A database error occurred while processing product data.",
                error: sqlEx.Message,
                data: new ProductSaveResult { Status = false, Message = sqlEx.Message, Prod_Id = product.Prod_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving product record. Prod_Id: {ProdId}", product.Prod_Id);
            return ApiResponse<ProductSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving product.",
                error: ex.Message,
                data: new ProductSaveResult { Status = false, Message = "Unexpected error occurred.", Prod_Id = product.Prod_Id });
        }
    }

    public async Task<ApiResponse<List<ProductListModel>>> GetAllProductsAsync(ProductFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Prod_Id", filter.Prod_Id, SqlDbType.Int),
                DbHelper.CreateParameter("@Prod_IsActive", filter.Prod_IsActive, SqlDbType.Bit)
            };

            var products = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Product_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<ProductListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapProductFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<ProductListModel>>.SuccessResult(
                data: products,
                message: $"Successfully retrieved {products.Count} product record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching products.");
            return ApiResponse<List<ProductListModel>>.FailureResult(
                message: "Unable to retrieve product list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching products.");
            return ApiResponse<List<ProductListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching products.",
                error: ex.Message);
        }
    }

    private static ProductListModel MapProductFromReader(SqlDataReader reader)
    {
        var model = new ProductListModel();

        if (HasColumn(reader, "Prod_Id") && !reader.IsDBNull(reader.GetOrdinal("Prod_Id")))
            model.Prod_Id = Convert.ToInt32(reader["Prod_Id"]);

        if (HasColumn(reader, "Prod_CompId") && !reader.IsDBNull(reader.GetOrdinal("Prod_CompId")))
            model.Prod_CompId = Convert.ToInt32(reader["Prod_CompId"]);

        if (HasColumn(reader, "Prod_CompanyName") && !reader.IsDBNull(reader.GetOrdinal("Prod_CompanyName")))
            model.Prod_CompanyName = Convert.ToString(reader["Prod_CompanyName"]);

        if (HasColumn(reader, "Prod_BranchId") && !reader.IsDBNull(reader.GetOrdinal("Prod_BranchId")))
            model.Prod_BranchId = Convert.ToInt32(reader["Prod_BranchId"]);

        if (HasColumn(reader, "Prod_BranchName") && !reader.IsDBNull(reader.GetOrdinal("Prod_BranchName")))
            model.Prod_BranchName = Convert.ToString(reader["Prod_BranchName"]);

        if (HasColumn(reader, "Prod_Code") && !reader.IsDBNull(reader.GetOrdinal("Prod_Code")))
            model.Prod_Code = Convert.ToString(reader["Prod_Code"]);

        if (HasColumn(reader, "Prod_Name") && !reader.IsDBNull(reader.GetOrdinal("Prod_Name")))
            model.Prod_Name = Convert.ToString(reader["Prod_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Prod_BrandId") && !reader.IsDBNull(reader.GetOrdinal("Prod_BrandId")))
            model.Prod_BrandId = Convert.ToInt32(reader["Prod_BrandId"]);

        if (HasColumn(reader, "Prod_BrandName") && !reader.IsDBNull(reader.GetOrdinal("Prod_BrandName")))
            model.Prod_BrandName = Convert.ToString(reader["Prod_BrandName"]);

        if (HasColumn(reader, "Prod_CategoryId") && !reader.IsDBNull(reader.GetOrdinal("Prod_CategoryId")))
            model.Prod_CategoryId = Convert.ToInt32(reader["Prod_CategoryId"]);

        if (HasColumn(reader, "Prod_CategoryName") && !reader.IsDBNull(reader.GetOrdinal("Prod_CategoryName")))
            model.Prod_CategoryName = Convert.ToString(reader["Prod_CategoryName"]);

        if (HasColumn(reader, "Prod_SubCategoryId") && !reader.IsDBNull(reader.GetOrdinal("Prod_SubCategoryId")))
            model.Prod_SubCategoryId = Convert.ToInt32(reader["Prod_SubCategoryId"]);

        if (HasColumn(reader, "Prod_SubCategoryName") && !reader.IsDBNull(reader.GetOrdinal("Prod_SubCategoryName")))
            model.Prod_SubCategoryName = Convert.ToString(reader["Prod_SubCategoryName"]);

        if (HasColumn(reader, "Prod_UnitId") && !reader.IsDBNull(reader.GetOrdinal("Prod_UnitId")))
            model.Prod_UnitId = Convert.ToInt32(reader["Prod_UnitId"]);

        if (HasColumn(reader, "Prod_UnitName") && !reader.IsDBNull(reader.GetOrdinal("Prod_UnitName")))
            model.Prod_UnitName = Convert.ToString(reader["Prod_UnitName"]);

        if (HasColumn(reader, "Prod_UnitShortName") && !reader.IsDBNull(reader.GetOrdinal("Prod_UnitShortName")))
            model.Prod_UnitShortName = Convert.ToString(reader["Prod_UnitShortName"]);

        if (HasColumn(reader, "Prod_UnitValue") && !reader.IsDBNull(reader.GetOrdinal("Prod_UnitValue")))
            model.Prod_UnitValue = Convert.ToDecimal(reader["Prod_UnitValue"]);

        if (HasColumn(reader, "Prod_HSNCode") && !reader.IsDBNull(reader.GetOrdinal("Prod_HSNCode")))
            model.Prod_HSNCode = Convert.ToString(reader["Prod_HSNCode"]);

        if (HasColumn(reader, "Prod_GSTPercent") && !reader.IsDBNull(reader.GetOrdinal("Prod_GSTPercent")))
            model.Prod_GSTPercent = Convert.ToDecimal(reader["Prod_GSTPercent"]);

        if (HasColumn(reader, "Batch_Barcode") && !reader.IsDBNull(reader.GetOrdinal("Batch_Barcode")))
            model.Batch_Barcode = Convert.ToString(reader["Batch_Barcode"]);

        if (HasColumn(reader, "Batch_EANCode") && !reader.IsDBNull(reader.GetOrdinal("Batch_EANCode")))
            model.Batch_EANCode = Convert.ToString(reader["Batch_EANCode"]);

        if (HasColumn(reader, "Batch_Stock") && !reader.IsDBNull(reader.GetOrdinal("Batch_Stock")))
            model.Batch_Stock = Convert.ToDecimal(reader["Batch_Stock"]);

        if (HasColumn(reader, "Batch_LandingPrice") && !reader.IsDBNull(reader.GetOrdinal("Batch_LandingPrice")))
            model.Batch_LandingPrice = Convert.ToDecimal(reader["Batch_LandingPrice"]);

        if (HasColumn(reader, "Batch_PurchasePrice") && !reader.IsDBNull(reader.GetOrdinal("Batch_PurchasePrice")))
            model.Batch_PurchasePrice = Convert.ToDecimal(reader["Batch_PurchasePrice"]);

        if (HasColumn(reader, "Batch_MRP") && !reader.IsDBNull(reader.GetOrdinal("Batch_MRP")))
            model.Batch_MRP = Convert.ToDecimal(reader["Batch_MRP"]);

        if (HasColumn(reader, "Batch_SellingPrice") && !reader.IsDBNull(reader.GetOrdinal("Batch_SellingPrice")))
            model.Batch_SellingPrice = Convert.ToDecimal(reader["Batch_SellingPrice"]);

        if (HasColumn(reader, "Prod_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Prod_IsActive")))
            model.Prod_IsActive = Convert.ToBoolean(reader["Prod_IsActive"]);

        if (HasColumn(reader, "Prod_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("Prod_CreatedBy")))
            model.Prod_CreatedBy = Convert.ToInt32(reader["Prod_CreatedBy"]);

        if (HasColumn(reader, "Prod_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Prod_CreatedDate")))
            model.Prod_CreatedDate = Convert.ToDateTime(reader["Prod_CreatedDate"]);

        if (HasColumn(reader, "Prod_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("Prod_ModifiedBy")))
            model.Prod_ModifiedBy = Convert.ToInt32(reader["Prod_ModifiedBy"]);

        if (HasColumn(reader, "Prod_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Prod_ModifiedDate")))
            model.Prod_ModifiedDate = Convert.ToDateTime(reader["Prod_ModifiedDate"]);

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
