using System.Data;
using System.Text.Json;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class ReceiptEntryRepository : IReceiptEntryRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<ReceiptEntryRepository> _logger;

    public ReceiptEntryRepository(DbHelper dbHelper, ILogger<ReceiptEntryRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<ReceiptEntrySaveResult>> SaveReceiptEntryAsync(ReceiptEntrySaveRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null
            };
            
            string masterDataJson = JsonSerializer.Serialize(request.MasterData, jsonOptions);
            string detailDataJson = JsonSerializer.Serialize(request.DetailData, jsonOptions);

            var parameters = new[]
            {
                new SqlParameter("@MasterDataJson", SqlDbType.NVarChar, -1) 
                { 
                    Value = string.IsNullOrWhiteSpace(masterDataJson) ? DBNull.Value : masterDataJson 
                },
                new SqlParameter("@DetailDataJson", SqlDbType.NVarChar, -1) 
                { 
                    Value = string.IsNullOrWhiteSpace(detailDataJson) ? DBNull.Value : detailDataJson 
                }
            };

            var saveResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_ReceiptEntry_InsertOrUpdate",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new ReceiptEntrySaveResult();
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (colName.Equals("Success", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                var statusVal = reader.GetValue(i);
                                result.Status = statusVal is bool b ? b : Convert.ToInt32(statusVal) == 1;
                            }
                            else if (colName.Equals("Message", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Message = Convert.ToString(reader.GetValue(i)) ?? string.Empty;
                            }
                            else if (colName.Equals("ReceiptMaster_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.ReceiptMaster_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                            else if (colName.Equals("ReceiptMaster_ReceiptNo", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.ReceiptMaster_ReceiptNo = Convert.ToString(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<ReceiptEntrySaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<ReceiptEntrySaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save receipt entry." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving receipt entry.");
            return ApiResponse<ReceiptEntrySaveResult>.FailureResult(
                message: "A database error occurred while processing receipt data.",
                error: sqlEx.Message,
                data: new ReceiptEntrySaveResult { Status = false, Message = sqlEx.Message, ReceiptMaster_Id = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving receipt entry.");
            return ApiResponse<ReceiptEntrySaveResult>.FailureResult(
                message: "An unexpected error occurred while saving receipt entry.",
                error: ex.Message,
                data: new ReceiptEntrySaveResult { Status = false, Message = "Unexpected error occurred.", ReceiptMaster_Id = 0 });
        }
    }
}
