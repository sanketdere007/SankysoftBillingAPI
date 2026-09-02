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

    public async Task<ApiResponse<List<CollectionReportResponse>>> GetCollectionReportAsync(CollectionReportRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@CompId", request.CompId, SqlDbType.Int),
                DbHelper.CreateParameter("@BranchId", request.BranchId, SqlDbType.Int),
                DbHelper.CreateParameter("@CustomerId", request.CustomerId, SqlDbType.Int),
                DbHelper.CreateParameter("@FromDate", request.FromDate, SqlDbType.Date),
                DbHelper.CreateParameter("@ToDate", request.ToDate, SqlDbType.Date),
                DbHelper.CreateParameter("@PaymentMode", request.PaymentMode, SqlDbType.NVarChar, 50),
                DbHelper.CreateParameter("@Search", request.Search, SqlDbType.NVarChar, 200),
                DbHelper.CreateParameter("@PageNumber", request.PageNumber, SqlDbType.Int),
                DbHelper.CreateParameter("@PageSize", request.PageSize, SqlDbType.Int)
            };

            var list = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_ReceiptEntry_CollectionReport",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var results = new List<CollectionReportResponse>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var row = new CollectionReportResponse();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.IsDBNull(i)) continue;

                            var colName = reader.GetName(i);
                            var val = reader.GetValue(i);

                            if (colName.Equals("ReceiptMaster_Id", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_Id = Convert.ToInt32(val);
                            else if (colName.Equals("ReceiptMaster_ReceiptNo", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_ReceiptNo = val.ToString();
                            else if (colName.Equals("ReceiptMaster_ReceiptDate", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_ReceiptDate = Convert.ToDateTime(val);
                            else if (colName.Equals("ReceiptMaster_Status", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_Status = val.ToString();
                            else if (colName.Equals("ReceiptMaster_IsActive", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_IsActive = Convert.ToBoolean(val);
                            else if (colName.Equals("ReceiptMaster_CompId", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_CompId = Convert.ToInt32(val);
                            else if (colName.Equals("Comp_Id", StringComparison.OrdinalIgnoreCase)) row.Comp_Id = Convert.ToInt32(val);
                            else if (colName.Equals("Comp_Name", StringComparison.OrdinalIgnoreCase)) row.Comp_Name = val.ToString();
                            else if (colName.Equals("ReceiptMaster_BranchId", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_BranchId = Convert.ToInt32(val);
                            else if (colName.Equals("Branch_Id", StringComparison.OrdinalIgnoreCase)) row.Branch_Id = Convert.ToInt32(val);
                            else if (colName.Equals("Branch_Name", StringComparison.OrdinalIgnoreCase)) row.Branch_Name = val.ToString();
                            else if (colName.Equals("ReceiptMaster_CustomerId", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_CustomerId = Convert.ToInt32(val);
                            else if (colName.Equals("Cust_Code", StringComparison.OrdinalIgnoreCase)) row.Cust_Code = val.ToString();
                            else if (colName.Equals("Cust_Name", StringComparison.OrdinalIgnoreCase)) row.Cust_Name = val.ToString();
                            else if (colName.Equals("Cust_MobileNo", StringComparison.OrdinalIgnoreCase)) row.Cust_MobileNo = val.ToString();
                            else if (colName.Equals("Cust_Email", StringComparison.OrdinalIgnoreCase)) row.Cust_Email = val.ToString();
                            else if (colName.Equals("ReceiptMaster_LedgerId", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_LedgerId = Convert.ToInt32(val);
                            else if (colName.Equals("AccLedger_Name", StringComparison.OrdinalIgnoreCase)) row.AccLedger_Name = val.ToString();
                            else if (colName.Equals("TotalCollection", StringComparison.OrdinalIgnoreCase)) row.TotalCollection = Convert.ToDecimal(val);
                            else if (colName.Equals("CashAmount", StringComparison.OrdinalIgnoreCase)) row.CashAmount = Convert.ToDecimal(val);
                            else if (colName.Equals("UPIAmount", StringComparison.OrdinalIgnoreCase)) row.UPIAmount = Convert.ToDecimal(val);
                            else if (colName.Equals("CardAmount", StringComparison.OrdinalIgnoreCase)) row.CardAmount = Convert.ToDecimal(val);
                            else if (colName.Equals("ChequeAmount", StringComparison.OrdinalIgnoreCase)) row.ChequeAmount = Convert.ToDecimal(val);
                            else if (colName.Equals("BankAmount", StringComparison.OrdinalIgnoreCase)) row.BankAmount = Convert.ToDecimal(val);
                            else if (colName.Equals("OtherAmount", StringComparison.OrdinalIgnoreCase)) row.OtherAmount = Convert.ToDecimal(val);
                            else if (colName.Equals("ReceiptMaster_ChequeNo", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_ChequeNo = val.ToString();
                            else if (colName.Equals("ReceiptMaster_ChequeDate", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_ChequeDate = Convert.ToDateTime(val);
                            else if (colName.Equals("ReceiptMaster_BankName", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_BankName = val.ToString();
                            else if (colName.Equals("ReceiptMaster_BankReferenceNo", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_BankReferenceNo = val.ToString();
                            else if (colName.Equals("ReceiptMaster_NEFTType", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_NEFTType = val.ToString();
                            else if (colName.Equals("ReceiptMaster_NEFTReferenceNo", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_NEFTReferenceNo = val.ToString();
                            else if (colName.Equals("ReceiptMaster_OtherPaymentType", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_OtherPaymentType = val.ToString();
                            else if (colName.Equals("ReceiptMaster_OtherReferenceNo", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_OtherReferenceNo = val.ToString();
                            else if (colName.Equals("ReceiptMaster_OtherDate", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_OtherDate = Convert.ToDateTime(val);
                            else if (colName.Equals("ReceiptMaster_OtherRemark", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_OtherRemark = val.ToString();
                            else if (colName.Equals("ReceiptMaster_Remark", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_Remark = val.ToString();
                            else if (colName.Equals("ReceiptMaster_CreatedBy", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_CreatedBy = Convert.ToInt32(val);
                            else if (colName.Equals("ReceiptMaster_CreatedDate", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_CreatedDate = Convert.ToDateTime(val);
                            else if (colName.Equals("ReceiptMaster_ModifiedBy", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_ModifiedBy = Convert.ToInt32(val);
                            else if (colName.Equals("ReceiptMaster_ModifiedDate", StringComparison.OrdinalIgnoreCase)) row.ReceiptMaster_ModifiedDate = Convert.ToDateTime(val);
                            else if (colName.Equals("CurrentPage", StringComparison.OrdinalIgnoreCase)) row.CurrentPage = Convert.ToInt32(val);
                            else if (colName.Equals("PageSize", StringComparison.OrdinalIgnoreCase)) row.PageSize = Convert.ToInt32(val);
                        }
                        results.Add(row);
                    }
                    return results;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<CollectionReportResponse>>.SuccessResult(list, "Report fetched successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching collection report.");
            return ApiResponse<List<CollectionReportResponse>>.FailureResult(
                message: "A database error occurred while fetching collection report.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching collection report.");
            return ApiResponse<List<CollectionReportResponse>>.FailureResult(
                message: "An unexpected error occurred while fetching collection report.",
                error: ex.Message);
        }
    }
}
