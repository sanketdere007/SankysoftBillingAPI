using System.Data;
using System.Text.Json;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class PurchaseEntryRepository : IPurchaseEntryRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<PurchaseEntryRepository> _logger;

    public PurchaseEntryRepository(DbHelper dbHelper, ILogger<PurchaseEntryRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<PurchaseEntrySaveResult>> SavePurchaseEntryAsync(PurchaseEntrySaveRequest request, CancellationToken cancellationToken = default)
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
                new SqlParameter("@DetailJson", SqlDbType.NVarChar, -1) 
                { 
                    Value = string.IsNullOrWhiteSpace(detailDataJson) ? DBNull.Value : detailDataJson 
                }
            };

            var saveResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_PurchaseEntry_InsertOrUpdate",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new PurchaseEntrySaveResult();
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
                            else if (colName.Equals("PurchaseMaster_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.PurchaseMaster_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<PurchaseEntrySaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<PurchaseEntrySaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save purchase entry." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving purchase entry.");
            return ApiResponse<PurchaseEntrySaveResult>.FailureResult(
                message: "A database error occurred while processing purchase data.",
                error: sqlEx.Message,
                data: new PurchaseEntrySaveResult { Status = false, Message = sqlEx.Message, PurchaseMaster_Id = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving purchase entry.");
            return ApiResponse<PurchaseEntrySaveResult>.FailureResult(
                message: "An unexpected error occurred while saving purchase entry.",
                error: ex.Message,
                data: new PurchaseEntrySaveResult { Status = false, Message = "Unexpected error occurred.", PurchaseMaster_Id = 0 });
        }
    }
    public async Task<ApiResponse<PagedListResult<PurchaseMasterViewModel>>> GetPurchaseMasterViewListAsync(PurchaseMasterViewListRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@CompId", request.CompId, SqlDbType.Int),
                DbHelper.CreateParameter("@BranchId", request.BranchId, SqlDbType.Int),
                DbHelper.CreateParameter("@SearchText", request.SearchText, SqlDbType.NVarChar, 200),
                DbHelper.CreateParameter("@FromDate", request.FromDate, SqlDbType.Date),
                DbHelper.CreateParameter("@ToDate", request.ToDate, SqlDbType.Date),
                DbHelper.CreateParameter("@PageNumber", request.PageNumber, SqlDbType.Int),
                DbHelper.CreateParameter("@PageSize", request.PageSize, SqlDbType.Int)
            };

            var list = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_PurchaseEntry_MasterViewList",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var resultList = new List<PurchaseMasterViewModel>();
                    int totalRecords = 0;

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var model = new PurchaseMasterViewModel();
                        
                        // Parse model properties
                        model.PurchaseMaster_Id = reader.GetInt32(reader.GetOrdinal("PurchaseMaster_Id"));
                        model.PurchaseMaster_CompId = reader.GetInt32(reader.GetOrdinal("PurchaseMaster_CompId"));
                        model.PurchaseMaster_BranchId = reader.GetInt32(reader.GetOrdinal("PurchaseMaster_BranchId"));
                        model.PurchaseMaster_SupplierId = reader.GetInt32(reader.GetOrdinal("PurchaseMaster_SupplierId"));
                        
                        var ledgerIdOrdinal = reader.GetOrdinal("PurchaseMaster_LedgerId");
                        if (!reader.IsDBNull(ledgerIdOrdinal)) model.PurchaseMaster_LedgerId = reader.GetInt32(ledgerIdOrdinal);

                        var invoiceNoOrdinal = reader.GetOrdinal("PurchaseMaster_InvoiceNo");
                        if (!reader.IsDBNull(invoiceNoOrdinal)) model.PurchaseMaster_InvoiceNo = reader.GetString(invoiceNoOrdinal);

                        var invoiceDateOrdinal = reader.GetOrdinal("PurchaseMaster_InvoiceDate");
                        if (!reader.IsDBNull(invoiceDateOrdinal)) model.PurchaseMaster_InvoiceDate = reader.GetDateTime(invoiceDateOrdinal);

                        model.PurchaseMaster_SubTotal = reader.GetDecimal(reader.GetOrdinal("PurchaseMaster_SubTotal"));
                        model.PurchaseMaster_DiscountAmount = reader.GetDecimal(reader.GetOrdinal("PurchaseMaster_DiscountAmount"));
                        model.PurchaseMaster_GSTAmount = reader.GetDecimal(reader.GetOrdinal("PurchaseMaster_GSTAmount"));
                        model.PurchaseMaster_OtherCharges = reader.GetDecimal(reader.GetOrdinal("PurchaseMaster_OtherCharges"));
                        model.PurchaseMaster_NetAmount = reader.GetDecimal(reader.GetOrdinal("PurchaseMaster_NetAmount"));

                        model.PurchaseMaster_PaidAmount = reader.GetDecimal(reader.GetOrdinal("PurchaseMaster_PaidAmount"));
                        model.PurchaseMaster_BalanceAmount = reader.GetDecimal(reader.GetOrdinal("PurchaseMaster_BalanceAmount"));

                        var statusOrdinal = reader.GetOrdinal("PurchaseMaster_Status");
                        if (!reader.IsDBNull(statusOrdinal)) model.PurchaseMaster_Status = reader.GetString(statusOrdinal);

                        var remarkOrdinal = reader.GetOrdinal("PurchaseMaster_Remark");
                        if (!reader.IsDBNull(remarkOrdinal)) model.PurchaseMaster_Remark = reader.GetString(remarkOrdinal);

                        model.PurchaseMaster_CreatedBy = reader.GetInt32(reader.GetOrdinal("PurchaseMaster_CreatedBy"));
                        
                        var createdDateOrdinal = reader.GetOrdinal("PurchaseMaster_CreatedDate");
                        if (!reader.IsDBNull(createdDateOrdinal)) model.PurchaseMaster_CreatedDate = reader.GetDateTime(createdDateOrdinal);

                        model.PurchaseMaster_ModifiedBy = reader.GetInt32(reader.GetOrdinal("PurchaseMaster_ModifiedBy"));
                        
                        var modifiedDateOrdinal = reader.GetOrdinal("PurchaseMaster_ModifiedDate");
                        if (!reader.IsDBNull(modifiedDateOrdinal)) model.PurchaseMaster_ModifiedDate = reader.GetDateTime(modifiedDateOrdinal);

                        var suppNameOrdinal = reader.GetOrdinal("Supp_Name");
                        if (!reader.IsDBNull(suppNameOrdinal)) model.Supp_Name = reader.GetString(suppNameOrdinal);

                        var branchNameOrdinal = reader.GetOrdinal("Branch_Name");
                        if (!reader.IsDBNull(branchNameOrdinal)) model.Branch_Name = reader.GetString(branchNameOrdinal);

                        var ledgerNameOrdinal = reader.GetOrdinal("AccLedger_Name");
                        if (!reader.IsDBNull(ledgerNameOrdinal)) model.AccLedger_Name = reader.GetString(ledgerNameOrdinal);

                        model.TotalRecords = reader.GetInt32(reader.GetOrdinal("TotalRecords"));
                        totalRecords = model.TotalRecords;

                        resultList.Add(model);
                    }

                    return new PagedListResult<PurchaseMasterViewModel>
                    {
                        Items = resultList,
                        PageSize = request.PageSize
                    };
                },
                cancellationToken: cancellationToken);

            return ApiResponse<PagedListResult<PurchaseMasterViewModel>>.SuccessResult(list, "Records retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while retrieving purchase master view list.");
            return ApiResponse<PagedListResult<PurchaseMasterViewModel>>.FailureResult("A database error occurred.", sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving purchase master view list.");
            return ApiResponse<PagedListResult<PurchaseMasterViewModel>>.FailureResult("An unexpected error occurred.", ex.Message);
        }
    }

    public async Task<ApiResponse<List<PurchaseDetailViewModel>>> GetPurchaseDetailViewListAsync(int purchaseMasterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@PurchaseMaster_Id", purchaseMasterId, SqlDbType.Int)
            };

            var list = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_PurchaseEntry_DetailViewList",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var resultList = new List<PurchaseDetailViewModel>();

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var model = new PurchaseDetailViewModel();

                        model.PurchaseDetail_Id = reader.GetInt32(reader.GetOrdinal("PurchaseDetail_Id"));
                        model.PurchaseDetail_MasterId = reader.GetInt32(reader.GetOrdinal("PurchaseDetail_MasterId"));
                        model.PurchaseDetail_CompId = reader.GetInt32(reader.GetOrdinal("PurchaseDetail_CompId"));
                        model.PurchaseDetail_BranchId = reader.GetInt32(reader.GetOrdinal("PurchaseDetail_BranchId"));
                        model.PurchaseDetail_ProductId = reader.GetInt32(reader.GetOrdinal("PurchaseDetail_ProductId"));

                        var barcodeOrdinal = reader.GetOrdinal("PurchaseDetail_Barcode");
                        if (!reader.IsDBNull(barcodeOrdinal)) model.PurchaseDetail_Barcode = reader.GetString(barcodeOrdinal);

                        var eanCodeOrdinal = reader.GetOrdinal("PurchaseDetail_EANCode");
                        if (!reader.IsDBNull(eanCodeOrdinal)) model.PurchaseDetail_EANCode = reader.GetString(eanCodeOrdinal);

                        model.PurchaseDetail_Qty = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_Qty"));

                        model.PurchaseDetail_LandingPrice = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_LandingPrice"));
                        model.PurchaseDetail_PurchasePrice = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_PurchasePrice"));
                        model.PurchaseDetail_MRP = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_MRP"));
                        model.PurchaseDetail_SellingPrice = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_SellingPrice"));

                        model.PurchaseDetail_DiscountPercent = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_DiscountPercent"));
                        model.PurchaseDetail_DiscountAmount = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_DiscountAmount"));

                        model.PurchaseDetail_GSTPercent = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_GSTPercent"));
                        model.PurchaseDetail_GSTAmount = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_GSTAmount"));

                        model.PurchaseDetail_TotalAmount = reader.GetDecimal(reader.GetOrdinal("PurchaseDetail_TotalAmount"));

                        model.PurchaseDetail_CreatedBy = reader.GetInt32(reader.GetOrdinal("PurchaseDetail_CreatedBy"));

                        var createdDateOrdinal = reader.GetOrdinal("PurchaseDetail_CreatedDate");
                        if (!reader.IsDBNull(createdDateOrdinal)) model.PurchaseDetail_CreatedDate = reader.GetDateTime(createdDateOrdinal);

                        model.PurchaseDetail_ModifiedBy = reader.GetInt32(reader.GetOrdinal("PurchaseDetail_ModifiedBy"));

                        var modifiedDateOrdinal = reader.GetOrdinal("PurchaseDetail_ModifiedDate");
                        if (!reader.IsDBNull(modifiedDateOrdinal)) model.PurchaseDetail_ModifiedDate = reader.GetDateTime(modifiedDateOrdinal);

                        var prodNameOrdinal = reader.GetOrdinal("Prod_Name");
                        if (!reader.IsDBNull(prodNameOrdinal)) model.Prod_Name = reader.GetString(prodNameOrdinal);

                        resultList.Add(model);
                    }

                    return resultList;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<PurchaseDetailViewModel>>.SuccessResult(list, "Records retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while retrieving purchase detail view list.");
            return ApiResponse<List<PurchaseDetailViewModel>>.FailureResult("A database error occurred.", sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving purchase detail view list.");
            return ApiResponse<List<PurchaseDetailViewModel>>.FailureResult("An unexpected error occurred.", ex.Message);
        }
    }
}
