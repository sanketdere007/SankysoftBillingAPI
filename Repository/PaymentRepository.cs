using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class PaymentRepository : IPaymentRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(DbHelper dbHelper, ILogger<PaymentRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<PaymentSaveResult>> SavePaymentEntryAsync(PaymentModel payment, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null 
            };
            var jsonData = JsonSerializer.Serialize(payment, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_PaymentEntry_InsertOrUpdate",
                jsonParameterName: "@PaymentDataJson",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new PaymentSaveResult();
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
                            else if (colName.Equals("PaymentMaster_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.PaymentMaster_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                            else if (colName.Equals("PaymentMaster_PaymentNo", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.PaymentMaster_PaymentNo = Convert.ToString(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<PaymentSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<PaymentSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save payment record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving payment record. PaymentMaster_Id: {PaymentId}", payment.PaymentMaster_Id);
            return ApiResponse<PaymentSaveResult>.FailureResult(
                message: "A database error occurred while processing payment data.",
                error: sqlEx.Message,
                data: new PaymentSaveResult { Status = false, Message = sqlEx.Message, PaymentMaster_Id = payment.PaymentMaster_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving payment record. PaymentMaster_Id: {PaymentId}", payment.PaymentMaster_Id);
            return ApiResponse<PaymentSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving payment.",
                error: ex.Message,
                data: new PaymentSaveResult { Status = false, Message = "Unexpected error occurred.", PaymentMaster_Id = payment.PaymentMaster_Id });
        }
    }

    public async Task<ApiResponse<PaymentEntryDeleteResult>> DeletePaymentEntryAsync(int paymentMasterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@PaymentMaster_Id", SqlDbType.Int) { Value = paymentMasterId }
            };

            var deleteResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_PaymentEntry_Delete",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new PaymentEntryDeleteResult { PaymentMaster_Id = paymentMasterId };
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
                            else if (colName.Equals("PaymentMaster_InvoiceNo", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.PaymentMaster_InvoiceNo = Convert.ToString(reader.GetValue(i));
                            }
                            else if (colName.Equals("ReversedPaymentAmount", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.ReversedPaymentAmount = Convert.ToDecimal(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (deleteResult.Status)
            {
                return ApiResponse<PaymentEntryDeleteResult>.SuccessResult(deleteResult, deleteResult.Message);
            }

            return ApiResponse<PaymentEntryDeleteResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(deleteResult.Message) ? "Failed to delete payment entry." : deleteResult.Message,
                error: null,
                data: deleteResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while deleting payment record. PaymentMaster_Id: {PaymentId}", paymentMasterId);
            return ApiResponse<PaymentEntryDeleteResult>.FailureResult(
                message: "A database error occurred while deleting payment data.",
                error: sqlEx.Message,
                data: new PaymentEntryDeleteResult { Status = false, Message = sqlEx.Message, PaymentMaster_Id = paymentMasterId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting payment record. PaymentMaster_Id: {PaymentId}", paymentMasterId);
            return ApiResponse<PaymentEntryDeleteResult>.FailureResult(
                message: "An unexpected error occurred while deleting payment entry.",
                error: ex.Message,
                data: new PaymentEntryDeleteResult { Status = false, Message = "Unexpected error occurred.", PaymentMaster_Id = paymentMasterId });
        }
    }
}
