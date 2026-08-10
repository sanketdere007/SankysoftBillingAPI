using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<CategoryRepository> _logger;

    public CategoryRepository(DbHelper dbHelper, ILogger<CategoryRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<CategorySaveResult>> SaveCategoryAsync(CategoryModel category, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null
            };
            var jsonData = JsonSerializer.Serialize(category, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_Category_InsertOrUpdate",
                jsonParameterName: "@CatJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new CategorySaveResult();
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
                            else if (colName.Equals("Cat_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Cat_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<CategorySaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<CategorySaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save category record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving category record. Cat_Id: {CatId}", category.Cat_Id);
            return ApiResponse<CategorySaveResult>.FailureResult(
                message: "A database error occurred while processing category data.",
                error: sqlEx.Message,
                data: new CategorySaveResult { Status = false, Message = sqlEx.Message, Cat_Id = category.Cat_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving category record. Cat_Id: {CatId}", category.Cat_Id);
            return ApiResponse<CategorySaveResult>.FailureResult(
                message: "An unexpected error occurred while saving category.",
                error: ex.Message,
                data: new CategorySaveResult { Status = false, Message = "Unexpected error occurred.", Cat_Id = category.Cat_Id });
        }
    }

    public async Task<ApiResponse<List<CategoryListModel>>> GetAllCategoriesAsync(CategoryFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Cat_Id", filter?.Cat_Id ?? (object)DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@Cat_IsActive", filter?.Cat_IsActive.HasValue == true ? (object)filter.Cat_IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var categories = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Category_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<CategoryListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapCategoryFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<CategoryListModel>>.SuccessResult(
                data: categories,
                message: $"Successfully retrieved {categories.Count} category record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching categories.");
            return ApiResponse<List<CategoryListModel>>.FailureResult(
                message: "Unable to retrieve category list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching categories.");
            return ApiResponse<List<CategoryListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching categories.",
                error: ex.Message);
        }
    }

    private static CategoryListModel MapCategoryFromReader(SqlDataReader reader)
    {
        var model = new CategoryListModel();

        if (HasColumn(reader, "Cat_Id") && !reader.IsDBNull(reader.GetOrdinal("Cat_Id")))
            model.Cat_Id = Convert.ToInt32(reader["Cat_Id"]);

        if (HasColumn(reader, "Cat_Name") && !reader.IsDBNull(reader.GetOrdinal("Cat_Name")))
            model.Cat_Name = Convert.ToString(reader["Cat_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Cat_Description") && !reader.IsDBNull(reader.GetOrdinal("Cat_Description")))
            model.Cat_Description = Convert.ToString(reader["Cat_Description"]);

        if (HasColumn(reader, "Cat_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Cat_IsActive")))
            model.Cat_IsActive = Convert.ToBoolean(reader["Cat_IsActive"]);

        if (HasColumn(reader, "Cat_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("Cat_CreatedBy")))
            model.Cat_CreatedBy = Convert.ToInt32(reader["Cat_CreatedBy"]);

        if (HasColumn(reader, "Cat_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Cat_CreatedDate")))
            model.Cat_CreatedDate = Convert.ToDateTime(reader["Cat_CreatedDate"]);

        if (HasColumn(reader, "Cat_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("Cat_ModifiedBy")))
            model.Cat_ModifiedBy = Convert.ToInt32(reader["Cat_ModifiedBy"]);

        if (HasColumn(reader, "Cat_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Cat_ModifiedDate")))
            model.Cat_ModifiedDate = Convert.ToDateTime(reader["Cat_ModifiedDate"]);

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
