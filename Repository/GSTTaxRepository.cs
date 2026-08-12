using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class GSTTaxRepository : IGSTTaxRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<GSTTaxRepository> _logger;

    public GSTTaxRepository(DbHelper dbHelper, ILogger<GSTTaxRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<GSTTaxSaveResult>> SaveGSTTaxAsync(GSTTaxModel gstTax, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null
            };
            var jsonData = JsonSerializer.Serialize(gstTax, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_GSTTax_InsertOrUpdate",
                jsonParameterName: "@GSTTaxJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new GSTTaxSaveResult();
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
                            else if (colName.Equals("GSTTax_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.GSTTax_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<GSTTaxSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<GSTTaxSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save GST Tax record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving GST Tax record. GSTTax_Id: {GSTTaxId}", gstTax.GSTTax_Id);
            return ApiResponse<GSTTaxSaveResult>.FailureResult(
                message: "A database error occurred while processing GST Tax data.",
                error: sqlEx.Message,
                data: new GSTTaxSaveResult { Status = false, Message = sqlEx.Message, GSTTax_Id = gstTax.GSTTax_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving GST Tax record. GSTTax_Id: {GSTTaxId}", gstTax.GSTTax_Id);
            return ApiResponse<GSTTaxSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving GST Tax.",
                error: ex.Message,
                data: new GSTTaxSaveResult { Status = false, Message = "Unexpected error occurred.", GSTTax_Id = gstTax.GSTTax_Id });
        }
    }

    public async Task<ApiResponse<List<GSTTaxListModel>>> GetAllGSTTaxesAsync(GSTTaxFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@GSTTax_Id", filter?.GSTTax_Id ?? (object)DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@GSTTax_IsActive", filter?.GSTTax_IsActive.HasValue == true ? (object)filter.GSTTax_IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var gstTaxes = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_GSTTax_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<GSTTaxListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapGSTTaxFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<GSTTaxListModel>>.SuccessResult(
                data: gstTaxes,
                message: $"Successfully retrieved {gstTaxes.Count} GST Tax record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching GST Taxes.");
            return ApiResponse<List<GSTTaxListModel>>.FailureResult(
                message: "Unable to retrieve GST Tax list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching GST Taxes.");
            return ApiResponse<List<GSTTaxListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching GST Taxes.",
                error: ex.Message);
        }
    }

    private static GSTTaxListModel MapGSTTaxFromReader(SqlDataReader reader)
    {
        var model = new GSTTaxListModel();

        if (HasColumn(reader, "GSTTax_Id") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_Id")))
            model.GSTTax_Id = Convert.ToInt32(reader["GSTTax_Id"]);

        if (HasColumn(reader, "GSTTax_Name") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_Name")))
            model.GSTTax_Name = Convert.ToString(reader["GSTTax_Name"]) ?? string.Empty;

        if (HasColumn(reader, "GSTTax_Percentage") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_Percentage")))
            model.GSTTax_Percentage = Convert.ToDecimal(reader["GSTTax_Percentage"]);

        if (HasColumn(reader, "GSTTax_CGST") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_CGST")))
            model.GSTTax_CGST = Convert.ToDecimal(reader["GSTTax_CGST"]);

        if (HasColumn(reader, "GSTTax_SGST") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_SGST")))
            model.GSTTax_SGST = Convert.ToDecimal(reader["GSTTax_SGST"]);

        if (HasColumn(reader, "GSTTax_IGST") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_IGST")))
            model.GSTTax_IGST = Convert.ToDecimal(reader["GSTTax_IGST"]);

        if (HasColumn(reader, "GSTTax_IsActive") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_IsActive")))
            model.GSTTax_IsActive = Convert.ToBoolean(reader["GSTTax_IsActive"]);

        if (HasColumn(reader, "GSTTax_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_CreatedBy")))
            model.GSTTax_CreatedBy = Convert.ToInt32(reader["GSTTax_CreatedBy"]);

        if (HasColumn(reader, "GSTTax_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_CreatedDate")))
            model.GSTTax_CreatedDate = Convert.ToDateTime(reader["GSTTax_CreatedDate"]);

        if (HasColumn(reader, "GSTTax_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_ModifiedBy")))
            model.GSTTax_ModifiedBy = Convert.ToInt32(reader["GSTTax_ModifiedBy"]);

        if (HasColumn(reader, "GSTTax_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("GSTTax_ModifiedDate")))
            model.GSTTax_ModifiedDate = Convert.ToDateTime(reader["GSTTax_ModifiedDate"]);

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
