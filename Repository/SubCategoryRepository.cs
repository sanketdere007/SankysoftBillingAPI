using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class SubCategoryRepository : ISubCategoryRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<SubCategoryRepository> _logger;

    public SubCategoryRepository(DbHelper dbHelper, ILogger<SubCategoryRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<SubCategorySaveResult>> SaveSubCategoryAsync(SubCategoryModel subCategory, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null
            };
            var jsonData = JsonSerializer.Serialize(subCategory, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_SubCategory_InsertOrUpdate",
                jsonParameterName: "@SubCatJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new SubCategorySaveResult();
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
                            else if (colName.Equals("SubCat_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.SubCat_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<SubCategorySaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<SubCategorySaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save sub-category record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving sub-category record. SubCat_Id: {SubCatId}", subCategory.SubCat_Id);
            return ApiResponse<SubCategorySaveResult>.FailureResult(
                message: "A database error occurred while processing sub-category data.",
                error: sqlEx.Message,
                data: new SubCategorySaveResult { Status = false, Message = sqlEx.Message, SubCat_Id = subCategory.SubCat_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving sub-category record. SubCat_Id: {SubCatId}", subCategory.SubCat_Id);
            return ApiResponse<SubCategorySaveResult>.FailureResult(
                message: "An unexpected error occurred while saving sub-category.",
                error: ex.Message,
                data: new SubCategorySaveResult { Status = false, Message = "Unexpected error occurred.", SubCat_Id = subCategory.SubCat_Id });
        }
    }

    public async Task<ApiResponse<List<SubCategoryListModel>>> GetAllSubCategoriesAsync(SubCategoryFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@SubCat_Id", filter?.SubCat_Id ?? (object)DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@SubCat_IsActive", filter?.SubCat_IsActive.HasValue == true ? (object)filter.SubCat_IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var subCategories = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_SubCategory_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<SubCategoryListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapSubCategoryFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<SubCategoryListModel>>.SuccessResult(
                data: subCategories,
                message: $"Successfully retrieved {subCategories.Count} sub-category record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching sub-categories.");
            return ApiResponse<List<SubCategoryListModel>>.FailureResult(
                message: "Unable to retrieve sub-category list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching sub-categories.");
            return ApiResponse<List<SubCategoryListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching sub-categories.",
                error: ex.Message);
        }
    }

    private static SubCategoryListModel MapSubCategoryFromReader(SqlDataReader reader)
    {
        var model = new SubCategoryListModel();

        if (HasColumn(reader, "SubCat_Id") && !reader.IsDBNull(reader.GetOrdinal("SubCat_Id")))
            model.SubCat_Id = Convert.ToInt32(reader["SubCat_Id"]);

        if (HasColumn(reader, "SubCat_CatId") && !reader.IsDBNull(reader.GetOrdinal("SubCat_CatId")))
            model.SubCat_CatId = Convert.ToInt32(reader["SubCat_CatId"]);

        if (HasColumn(reader, "Cat_Name") && !reader.IsDBNull(reader.GetOrdinal("Cat_Name")))
            model.Cat_Name = Convert.ToString(reader["Cat_Name"]);

        if (HasColumn(reader, "SubCat_Name") && !reader.IsDBNull(reader.GetOrdinal("SubCat_Name")))
            model.SubCat_Name = Convert.ToString(reader["SubCat_Name"]) ?? string.Empty;

        if (HasColumn(reader, "SubCat_Description") && !reader.IsDBNull(reader.GetOrdinal("SubCat_Description")))
            model.SubCat_Description = Convert.ToString(reader["SubCat_Description"]);

        if (HasColumn(reader, "SubCat_IsActive") && !reader.IsDBNull(reader.GetOrdinal("SubCat_IsActive")))
            model.SubCat_IsActive = Convert.ToBoolean(reader["SubCat_IsActive"]);

        if (HasColumn(reader, "SubCat_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("SubCat_CreatedBy")))
            model.SubCat_CreatedBy = Convert.ToInt32(reader["SubCat_CreatedBy"]);

        if (HasColumn(reader, "SubCat_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("SubCat_CreatedDate")))
            model.SubCat_CreatedDate = Convert.ToDateTime(reader["SubCat_CreatedDate"]);

        if (HasColumn(reader, "SubCat_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("SubCat_ModifiedBy")))
            model.SubCat_ModifiedBy = Convert.ToInt32(reader["SubCat_ModifiedBy"]);

        if (HasColumn(reader, "SubCat_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("SubCat_ModifiedDate")))
            model.SubCat_ModifiedDate = Convert.ToDateTime(reader["SubCat_ModifiedDate"]);

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
