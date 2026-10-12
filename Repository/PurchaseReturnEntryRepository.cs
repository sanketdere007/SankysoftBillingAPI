using System.Data;
using System.Text.Json;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class PurchaseReturnEntryRepository : IPurchaseReturnEntryRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<PurchaseReturnEntryRepository> _logger;

    public PurchaseReturnEntryRepository(DbHelper dbHelper, ILogger<PurchaseReturnEntryRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<PurchaseReturnEntrySaveResult>> SavePurchaseReturnEntryAsync(PurchaseReturnEntrySaveRequest request, CancellationToken cancellationToken = default)
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
                new SqlParameter("@PurchaseReturnMaster_Id", SqlDbType.Int)
                {
                    Value = request.MasterData.PurchaseReturnMaster_Id
                },
                new SqlParameter("@MasterJson", SqlDbType.NVarChar, -1) 
                { 
                    Value = string.IsNullOrWhiteSpace(masterDataJson) ? DBNull.Value : masterDataJson 
                },
                new SqlParameter("@DetailJson", SqlDbType.NVarChar, -1) 
                { 
                    Value = string.IsNullOrWhiteSpace(detailDataJson) ? DBNull.Value : detailDataJson 
                }
            };

            var saveResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_PurchaseReturn_InsertOrUpdate",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new PurchaseReturnEntrySaveResult();
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
                            else if (colName.Equals("PurchaseReturnMaster_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.PurchaseReturnMaster_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                            else if (colName.Equals("PurchaseReturnMaster_ReturnNo", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.PurchaseReturnMaster_ReturnNo = Convert.ToString(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<PurchaseReturnEntrySaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<PurchaseReturnEntrySaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save purchase return entry." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving purchase return entry.");
            return ApiResponse<PurchaseReturnEntrySaveResult>.FailureResult(
                message: "A database error occurred while processing purchase return data.",
                error: sqlEx.Message,
                data: new PurchaseReturnEntrySaveResult { Status = false, Message = sqlEx.Message, PurchaseReturnMaster_Id = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving purchase return entry.");
            return ApiResponse<PurchaseReturnEntrySaveResult>.FailureResult(
                message: "An unexpected error occurred while saving purchase return entry.",
                error: ex.Message,
                data: new PurchaseReturnEntrySaveResult { Status = false, Message = "Unexpected error occurred.", PurchaseReturnMaster_Id = 0 });
        }
    }

    public async Task<ApiResponse<PagedListResult<PurchaseReturnMasterViewListResponse>>> GetPurchaseReturnMasterViewListAsync(PurchaseReturnMasterViewListRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@CompId", SqlDbType.Int) { Value = request.CompId },
                new SqlParameter("@BranchId", SqlDbType.Int) { Value = request.BranchId },
                new SqlParameter("@PurchaseReturnMaster_Id", SqlDbType.Int) { Value = request.PurchaseReturnMaster_Id ?? (object)DBNull.Value },
                new SqlParameter("@SupplierId", SqlDbType.Int) { Value = request.SupplierId ?? (object)DBNull.Value },
                new SqlParameter("@Search", SqlDbType.NVarChar, 200) { Value = request.Search ?? (object)DBNull.Value },
                new SqlParameter("@FromDate", SqlDbType.Date) { Value = request.FromDate ?? (object)DBNull.Value },
                new SqlParameter("@ToDate", SqlDbType.Date) { Value = request.ToDate ?? (object)DBNull.Value },
                new SqlParameter("@Page", SqlDbType.Int) { Value = request.Page },
                new SqlParameter("@PageSize", SqlDbType.Int) { Value = request.PageSize }
            };

            var dataList = new List<PurchaseReturnMasterViewListResponse>();

            await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_PurchaseReturn_MasterViewList",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var row = new PurchaseReturnMasterViewListResponse();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (reader.IsDBNull(i)) continue;

                            switch (colName)
                            {
                                case "PurchaseReturnMaster_Id": row.PurchaseReturnMaster_Id = reader.GetInt32(i); break;
                                case "PurchaseReturnMaster_CompId": row.PurchaseReturnMaster_CompId = reader.GetInt32(i); break;
                                case "PurchaseReturnMaster_BranchId": row.PurchaseReturnMaster_BranchId = reader.GetInt32(i); break;
                                case "PurchaseReturnMaster_SupplierId": row.PurchaseReturnMaster_SupplierId = reader.GetInt32(i); break;
                                case "SupplierCode": row.SupplierCode = reader.GetString(i); break;
                                case "SupplierMobile": row.SupplierMobile = reader.GetString(i); break;
                                case "PurchaseReturnMaster_LedgerId": row.PurchaseReturnMaster_LedgerId = reader.GetInt32(i); break;
                                case "LedgerName": row.LedgerName = reader.GetString(i); break;
                                case "PurchaseReturnMaster_ReturnNo": row.PurchaseReturnMaster_ReturnNo = reader.GetString(i); break;
                                case "PurchaseReturnMaster_ReturnDate": row.PurchaseReturnMaster_ReturnDate = reader.GetDateTime(i); break;
                                case "PurchaseReturnMaster_SubTotal": row.PurchaseReturnMaster_SubTotal = reader.GetDecimal(i); break;
                                case "PurchaseReturnMaster_DiscountAmount": row.PurchaseReturnMaster_DiscountAmount = reader.GetDecimal(i); break;
                                case "PurchaseReturnMaster_BillWiseDiscountPercentage": row.PurchaseReturnMaster_BillWiseDiscountPercentage = reader.GetDecimal(i); break;
                                case "PurchaseReturnMaster_BillWiseDiscountAmount": row.PurchaseReturnMaster_BillWiseDiscountAmount = reader.GetDecimal(i); break;
                                case "PurchaseReturnMaster_GSTAmount": row.PurchaseReturnMaster_GSTAmount = reader.GetDecimal(i); break;
                                case "PurchaseReturnMaster_OtherCharges": row.PurchaseReturnMaster_OtherCharges = reader.GetDecimal(i); break;
                                case "PurchaseReturnMaster_NetAmount": row.PurchaseReturnMaster_NetAmount = reader.GetDecimal(i); break;
                                case "PurchaseReturnMaster_RefundAmount": row.PurchaseReturnMaster_RefundAmount = reader.GetDecimal(i); break;
                                case "PurchaseReturnMaster_BalanceAmount": row.PurchaseReturnMaster_BalanceAmount = reader.GetDecimal(i); break;
                                case "PurchaseReturnMaster_Status": row.PurchaseReturnMaster_Status = reader.GetString(i); break;
                                case "PurchaseReturnMaster_Remark": row.PurchaseReturnMaster_Remark = reader.GetString(i); break;
                                case "PurchaseReturnMaster_CreatedBy": row.PurchaseReturnMaster_CreatedBy = reader.GetInt32(i); break;
                                case "PurchaseReturnMaster_CreatedDate": row.PurchaseReturnMaster_CreatedDate = reader.GetDateTime(i); break;
                                case "PurchaseReturnMaster_ModifiedBy": row.PurchaseReturnMaster_ModifiedBy = reader.GetInt32(i); break;
                                case "PurchaseReturnMaster_ModifiedDate": row.PurchaseReturnMaster_ModifiedDate = reader.GetDateTime(i); break;
                            }
                        }
                        dataList.Add(row);
                    }
                    return true;
                },
                cancellationToken: cancellationToken);

            var pagedResult = new PagedListResult<PurchaseReturnMasterViewListResponse>
            {
                Items = dataList,
                TotalRecords = dataList.Count > 0 ? -1 : 0 // TotalRecords may not be provided by SP, handling accordingly if they need it, or we could just set it as is.
            };

            return ApiResponse<PagedListResult<PurchaseReturnMasterViewListResponse>>.SuccessResult(pagedResult, "List retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while retrieving purchase return master list.");
            return ApiResponse<PagedListResult<PurchaseReturnMasterViewListResponse>>.FailureResult(
                message: "A database error occurred while retrieving data.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving purchase return master list.");
            return ApiResponse<PagedListResult<PurchaseReturnMasterViewListResponse>>.FailureResult(
                message: "An unexpected error occurred while retrieving data.",
                error: ex.Message);
        }
    }

    public async Task<ApiResponse<List<PurchaseReturnDetailViewListResponse>>> GetPurchaseReturnDetailViewListAsync(PurchaseReturnDetailViewListRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@PurchaseReturnMaster_Id", SqlDbType.Int) { Value = request.PurchaseReturnMaster_Id },
                new SqlParameter("@CompId", SqlDbType.Int) { Value = request.CompId },
                new SqlParameter("@BranchId", SqlDbType.Int) { Value = request.BranchId }
            };

            var dataList = new List<PurchaseReturnDetailViewListResponse>();

            await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_PurchaseReturn_DetailViewList",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    // The SP might return an error structure `Status, Message, ...` if not found. Let's check for it.
                    // Or it returns normal columns. Let's peek the first row/columns.
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var row = new PurchaseReturnDetailViewListResponse();

                        bool isErrorResult = false;
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (colName.Equals("Status", StringComparison.OrdinalIgnoreCase))
                            {
                                isErrorResult = true;
                                break;
                            }
                        }
                        if (isErrorResult) continue; // Skip mapping if it's the error result. The upper layer should ideally handle error sets better, but this SP returns an error set directly.
                        
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (reader.IsDBNull(i)) continue;

                            switch (colName)
                            {
                                case "PurchaseReturnDetail_Id": row.PurchaseReturnDetail_Id = reader.GetInt32(i); break;
                                case "PurchaseReturnDetail_MasterId": row.PurchaseReturnDetail_MasterId = reader.GetInt32(i); break;
                                case "PurchaseReturnDetail_CompId": row.PurchaseReturnDetail_CompId = reader.GetInt32(i); break;
                                case "PurchaseReturnDetail_BranchId": row.PurchaseReturnDetail_BranchId = reader.GetInt32(i); break;
                                case "PurchaseReturnDetail_ProductId": row.PurchaseReturnDetail_ProductId = reader.GetInt32(i); break;
                                case "ProductCode": row.ProductCode = reader.GetString(i); break;
                                case "ProductName": row.ProductName = reader.GetString(i); break;
                                case "PurchaseReturnDetail_BatchId": row.PurchaseReturnDetail_BatchId = reader.GetInt32(i); break;
                                case "BatchNumber": row.BatchNumber = reader.GetString(i); break;
                                case "BatchStock": row.BatchStock = reader.GetDecimal(i); break;
                                case "BatchAvailableStock": row.BatchAvailableStock = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_Qty": row.PurchaseReturnDetail_Qty = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_LandingPrice": row.PurchaseReturnDetail_LandingPrice = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_PurchasePrice": row.PurchaseReturnDetail_PurchasePrice = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_MRP": row.PurchaseReturnDetail_MRP = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_SellingPrice": row.PurchaseReturnDetail_SellingPrice = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_DiscountPercent": row.PurchaseReturnDetail_DiscountPercent = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_DiscountAmount": row.PurchaseReturnDetail_DiscountAmount = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_GSTPercent": row.PurchaseReturnDetail_GSTPercent = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_GSTAmount": row.PurchaseReturnDetail_GSTAmount = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_TotalAmount": row.PurchaseReturnDetail_TotalAmount = reader.GetDecimal(i); break;
                                case "PurchaseReturnDetail_CreatedBy": row.PurchaseReturnDetail_CreatedBy = reader.GetInt32(i); break;
                                case "PurchaseReturnDetail_CreatedDate": row.PurchaseReturnDetail_CreatedDate = reader.GetDateTime(i); break;
                                case "PurchaseReturnDetail_ModifiedBy": row.PurchaseReturnDetail_ModifiedBy = reader.GetInt32(i); break;
                                case "PurchaseReturnDetail_ModifiedDate": row.PurchaseReturnDetail_ModifiedDate = reader.GetDateTime(i); break;
                            }
                        }
                        dataList.Add(row);
                    }
                    return true;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<PurchaseReturnDetailViewListResponse>>.SuccessResult(dataList, "Detail list retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while retrieving purchase return detail list.");
            return ApiResponse<List<PurchaseReturnDetailViewListResponse>>.FailureResult(
                message: "A database error occurred while retrieving detail data.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving purchase return detail list.");
            return ApiResponse<List<PurchaseReturnDetailViewListResponse>>.FailureResult(
                message: "An unexpected error occurred while retrieving detail data.",
                error: ex.Message);
        }
    }
}
