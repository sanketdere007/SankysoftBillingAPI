using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class BrandRepository : IBrandRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<BrandRepository> _logger;

    public BrandRepository(DbHelper dbHelper, ILogger<BrandRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<BrandSaveResult>> SaveBrandAsync(BrandModel brand, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null
            };
            var jsonData = JsonSerializer.Serialize(brand, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_Brand_InsertOrUpdate",
                jsonParameterName: "@BrandJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new BrandSaveResult();
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
                            else if (colName.Equals("Brand_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Brand_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<BrandSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<BrandSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save brand record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving brand record. Brand_Id: {BrandId}", brand.Brand_Id);
            return ApiResponse<BrandSaveResult>.FailureResult(
                message: "A database error occurred while processing brand data.",
                error: sqlEx.Message,
                data: new BrandSaveResult { Status = false, Message = sqlEx.Message, Brand_Id = brand.Brand_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving brand record. Brand_Id: {BrandId}", brand.Brand_Id);
            return ApiResponse<BrandSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving brand.",
                error: ex.Message,
                data: new BrandSaveResult { Status = false, Message = "Unexpected error occurred.", Brand_Id = brand.Brand_Id });
        }
    }

    public async Task<ApiResponse<List<BrandListModel>>> GetAllBrandsAsync(BrandFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Brand_Id", filter?.Brand_Id ?? (object)DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@Brand_IsActive", filter?.Brand_IsActive.HasValue == true ? (object)filter.Brand_IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var brands = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Brand_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<BrandListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapBrandFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<BrandListModel>>.SuccessResult(
                data: brands,
                message: $"Successfully retrieved {brands.Count} brand record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching brands.");
            return ApiResponse<List<BrandListModel>>.FailureResult(
                message: "Unable to retrieve brand list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching brands.");
            return ApiResponse<List<BrandListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching brands.",
                error: ex.Message);
        }
    }

    private static BrandListModel MapBrandFromReader(SqlDataReader reader)
    {
        var model = new BrandListModel();

        if (HasColumn(reader, "Brand_Id") && !reader.IsDBNull(reader.GetOrdinal("Brand_Id")))
            model.Brand_Id = Convert.ToInt32(reader["Brand_Id"]);

        if (HasColumn(reader, "Brand_Name") && !reader.IsDBNull(reader.GetOrdinal("Brand_Name")))
            model.Brand_Name = Convert.ToString(reader["Brand_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Brand_Description") && !reader.IsDBNull(reader.GetOrdinal("Brand_Description")))
            model.Brand_Description = Convert.ToString(reader["Brand_Description"]);

        if (HasColumn(reader, "Brand_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Brand_IsActive")))
            model.Brand_IsActive = Convert.ToBoolean(reader["Brand_IsActive"]);

        if (HasColumn(reader, "Brand_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("Brand_CreatedBy")))
            model.Brand_CreatedBy = Convert.ToInt32(reader["Brand_CreatedBy"]);

        if (HasColumn(reader, "Brand_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Brand_CreatedDate")))
            model.Brand_CreatedDate = Convert.ToDateTime(reader["Brand_CreatedDate"]);

        if (HasColumn(reader, "Brand_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("Brand_ModifiedBy")))
            model.Brand_ModifiedBy = Convert.ToInt32(reader["Brand_ModifiedBy"]);

        if (HasColumn(reader, "Brand_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Brand_ModifiedDate")))
            model.Brand_ModifiedDate = Convert.ToDateTime(reader["Brand_ModifiedDate"]);

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
