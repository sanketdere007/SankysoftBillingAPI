using System.Data;
using System.Text.Json;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class SalesReturnEntryRepository : ISalesReturnEntryRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<SalesReturnEntryRepository> _logger;

    public SalesReturnEntryRepository(DbHelper dbHelper, ILogger<SalesReturnEntryRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<SalesReturnEntrySaveResult>> SaveSalesReturnEntryAsync(SalesReturnEntrySaveRequest request, CancellationToken cancellationToken = default)
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
                procedureName: "dbo.SP_SalesReturnEntry_InsertOrUpdate",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new SalesReturnEntrySaveResult();
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        result.Status = true; // Assume success if reader returns data and no exception is thrown
                        
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (colName.Equals("Message", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Message = Convert.ToString(reader.GetValue(i)) ?? string.Empty;
                            }
                            else if (colName.Equals("SalesReturnMaster_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.SalesReturnMaster_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                            else if (colName.Equals("SalesReturnMaster_InvoiceNo", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.SalesReturnMaster_InvoiceNo = Convert.ToString(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<SalesReturnEntrySaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<SalesReturnEntrySaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save sales return entry." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving sales return entry.");
            return ApiResponse<SalesReturnEntrySaveResult>.FailureResult(
                message: "A database error occurred while processing sales return data.",
                error: sqlEx.Message,
                data: new SalesReturnEntrySaveResult { Status = false, Message = sqlEx.Message, SalesReturnMaster_Id = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving sales return entry.");
            return ApiResponse<SalesReturnEntrySaveResult>.FailureResult(
                message: "An unexpected error occurred while saving sales return entry.",
                error: ex.Message,
                data: new SalesReturnEntrySaveResult { Status = false, Message = "Unexpected error occurred.", SalesReturnMaster_Id = 0 });
        }
    }
    public async Task<ApiResponse<SalesReturnEntryDeleteResult>> DeleteSalesReturnEntryAsync(int salesReturnMasterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@SalesReturnMaster_Id", salesReturnMasterId, SqlDbType.Int)
            };

            var deleteResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_SalesReturnEntry_Delete",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new SalesReturnEntryDeleteResult();
                    // Assume success if no error is thrown
                    result.Status = true;
                    
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
                            else if (colName.Equals("SalesReturnMaster_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.SalesReturnMaster_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                            else if (colName.Equals("ErrorNumber", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.ErrorNumber = Convert.ToInt32(reader.GetValue(i));
                                result.Status = false;
                            }
                            else if (colName.Equals("ErrorLine", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.ErrorLine = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (deleteResult.Status)
            {
                return ApiResponse<SalesReturnEntryDeleteResult>.SuccessResult(deleteResult, string.IsNullOrWhiteSpace(deleteResult.Message) ? "Deleted successfully." : deleteResult.Message);
            }

            return ApiResponse<SalesReturnEntryDeleteResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(deleteResult.Message) ? "Failed to delete sales return entry." : deleteResult.Message,
                error: null,
                data: deleteResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while deleting sales return entry.");
            return ApiResponse<SalesReturnEntryDeleteResult>.FailureResult(
                message: "A database error occurred while deleting sales return data.",
                error: sqlEx.Message,
                data: new SalesReturnEntryDeleteResult { Status = false, Message = sqlEx.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while deleting sales return entry.");
            return ApiResponse<SalesReturnEntryDeleteResult>.FailureResult(
                message: "An unexpected error occurred while deleting sales return entry.",
                error: ex.Message,
                data: new SalesReturnEntryDeleteResult { Status = false, Message = "Unexpected error occurred." });
        }
    }

    public async Task<ApiResponse<IEnumerable<SalesReturnListResponse>>> GetSalesReturnListAsync(SalesReturnListRequest request, CancellationToken cancellationToken = default)
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
                DbHelper.CreateParameter("@Search", request.Search, SqlDbType.NVarChar, 200),
                DbHelper.CreateParameter("@PageNumber", request.PageNumber, SqlDbType.Int),
                DbHelper.CreateParameter("@PageSize", request.PageSize, SqlDbType.Int)
            };

            var dataList = new List<SalesReturnListResponse>();

            await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_SalesReturnEntryMaster_List",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var row = new SalesReturnListResponse();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (reader.IsDBNull(i)) continue;

                            if (colName.Equals("SalesReturnMaster_Id", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_Id = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_CompId", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_CompId = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_BranchId", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_BranchId = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_InvoiceNo", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_InvoiceNo = Convert.ToString(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_Date", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_Date = Convert.ToDateTime(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_CustomerId", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_CustomerId = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_LedgerId", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_LedgerId = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_TotalQty", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_TotalQty = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_SubTotal", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_SubTotal = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_DiscountAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_DiscountAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_TaxAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_TaxAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_BillWiseDiscountPercentage", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_BillWiseDiscountPercentage = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_BillWiseDiscountAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_BillWiseDiscountAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_GrandTotal", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_GrandTotal = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_PaidAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_PaidAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_BalanceAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_BalanceAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_CashAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_CashAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_UPIAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_UPIAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_ChequeAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_ChequeAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_CreditAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_CreditAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_Status", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_Status = Convert.ToString(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_Remark", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_Remark = Convert.ToString(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_IsActive", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_IsActive = Convert.ToBoolean(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_CreatedBy", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_CreatedBy = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_CreatedDate", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_CreatedDate = Convert.ToDateTime(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_ModifiedBy", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_ModifiedBy = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnMaster_ModifiedDate", StringComparison.OrdinalIgnoreCase)) row.SalesReturnMaster_ModifiedDate = Convert.ToDateTime(reader.GetValue(i));
                            else if (colName.Equals("Cust_Name", StringComparison.OrdinalIgnoreCase)) row.Cust_Name = Convert.ToString(reader.GetValue(i));
                            else if (colName.Equals("Cust_MobileNo", StringComparison.OrdinalIgnoreCase)) row.Cust_MobileNo = Convert.ToString(reader.GetValue(i));
                        }

                        dataList.Add(row);
                    }
                    return true;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<IEnumerable<SalesReturnListResponse>>.SuccessResult(dataList, "Sales return list retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while retrieving sales return list.");
            return ApiResponse<IEnumerable<SalesReturnListResponse>>.FailureResult(
                message: "A database error occurred while retrieving data.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving sales return list.");
            return ApiResponse<IEnumerable<SalesReturnListResponse>>.FailureResult(
                message: "An unexpected error occurred while retrieving data.",
                error: ex.Message);
        }
    }

    public async Task<ApiResponse<IEnumerable<SalesReturnEntryDetailResponse>>> GetSalesReturnEntryDetailListAsync(int salesReturnMasterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@SalesReturnMaster_Id", salesReturnMasterId, SqlDbType.Int)
            };

            var dataList = new List<SalesReturnEntryDetailResponse>();

            await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_SalesReturnEntryDetail_List",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var row = new SalesReturnEntryDetailResponse();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (reader.IsDBNull(i)) continue;

                            if (colName.Equals("SalesReturnDetail_Id", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_Id = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_MasterId", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_MasterId = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_ProductId", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_ProductId = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("Prod_Name", StringComparison.OrdinalIgnoreCase)) row.Prod_Name = Convert.ToString(reader.GetValue(i));
                            else if (colName.Equals("Prod_Code", StringComparison.OrdinalIgnoreCase)) row.Prod_Code = Convert.ToString(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_BatchId", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_BatchId = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_UnitId", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_UnitId = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_Qty", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_Qty = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_FreeQty", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_FreeQty = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_SellingPrice", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_SellingPrice = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_MRP", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_MRP = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_DiscountPercentage", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_DiscountPercentage = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_DiscountAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_DiscountAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_TaxPercentage", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_TaxPercentage = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_TaxAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_TaxAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_SubTotal", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_SubTotal = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_TotalAmount", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_TotalAmount = Convert.ToDecimal(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_Remark", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_Remark = Convert.ToString(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_IsActive", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_IsActive = Convert.ToBoolean(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_CreatedDate", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_CreatedDate = Convert.ToDateTime(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_ModifiedBy", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_ModifiedBy = Convert.ToInt32(reader.GetValue(i));
                            else if (colName.Equals("SalesReturnDetail_ModifiedDate", StringComparison.OrdinalIgnoreCase)) row.SalesReturnDetail_ModifiedDate = Convert.ToDateTime(reader.GetValue(i));
                        }

                        dataList.Add(row);
                    }
                    return true;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<IEnumerable<SalesReturnEntryDetailResponse>>.SuccessResult(dataList, "Sales return details retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while retrieving sales return details.");
            return ApiResponse<IEnumerable<SalesReturnEntryDetailResponse>>.FailureResult(
                message: "A database error occurred while retrieving data.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving sales return details.");
            return ApiResponse<IEnumerable<SalesReturnEntryDetailResponse>>.FailureResult(
                message: "An unexpected error occurred while retrieving data.",
                error: ex.Message);
        }
    }
}
