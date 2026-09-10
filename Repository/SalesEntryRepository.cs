using System.Data;
using System.Text.Json;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class SalesEntryRepository : ISalesEntryRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<SalesEntryRepository> _logger;

    public SalesEntryRepository(DbHelper dbHelper, ILogger<SalesEntryRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<SalesEntrySaveResult>> SaveSalesEntryAsync(SalesEntrySaveRequest request, CancellationToken cancellationToken = default)
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
            
            // Serialize receipt data, use empty array/object representation if null, or DBNull as per design
            // Since the SP handles them, we should pass them as JSON strings or DBNull.Value.
            string receiptMasterJson = request.ReceiptMasterData != null 
                ? JsonSerializer.Serialize(request.ReceiptMasterData, jsonOptions) 
                : string.Empty;
            string receiptDetailJson = request.ReceiptDetailData != null && request.ReceiptDetailData.Count > 0
                ? JsonSerializer.Serialize(request.ReceiptDetailData, jsonOptions) 
                : string.Empty;

            var parameters = new[]
            {
                new SqlParameter("@MasterDataJson", SqlDbType.NVarChar, -1)
                {
                    Value = string.IsNullOrWhiteSpace(masterDataJson) ? DBNull.Value : masterDataJson
                },
                new SqlParameter("@DetailDataJson", SqlDbType.NVarChar, -1)
                {
                    Value = string.IsNullOrWhiteSpace(detailDataJson) ? DBNull.Value : detailDataJson
                },
                new SqlParameter("@ReceiptMasterJson", SqlDbType.NVarChar, -1)
                {
                    Value = string.IsNullOrWhiteSpace(receiptMasterJson) ? DBNull.Value : receiptMasterJson
                },
                new SqlParameter("@ReceiptDetailJson", SqlDbType.NVarChar, -1)
                {
                    Value = string.IsNullOrWhiteSpace(receiptDetailJson) ? DBNull.Value : receiptDetailJson
                }
            };

            var saveResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_SalesEntry_InsertOrUpdate",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new SalesEntrySaveResult();
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
                            else if (colName.Equals("SalesMaster_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.SalesMaster_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                            else if (colName.Equals("SalesMaster_InvoiceNo", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.SalesMaster_InvoiceNo = Convert.ToString(reader.GetValue(i));
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
                return ApiResponse<SalesEntrySaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<SalesEntrySaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save sales entry." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving sales entry.");
            return ApiResponse<SalesEntrySaveResult>.FailureResult(
                message: "A database error occurred while processing sales data.",
                error: sqlEx.Message,
                data: new SalesEntrySaveResult { Status = false, Message = sqlEx.Message, SalesMaster_Id = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving sales entry.");
            return ApiResponse<SalesEntrySaveResult>.FailureResult(
                message: "An unexpected error occurred while saving sales entry.",
                error: ex.Message,
                data: new SalesEntrySaveResult { Status = false, Message = "Unexpected error occurred.", SalesMaster_Id = 0 });
        }
    }

    public async Task<ApiResponse<PagedListResult<SalesMasterListModel>>> GetAllSalesMasterAsync(SalesMasterFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@CompId", filter.CompId, SqlDbType.Int),
                DbHelper.CreateParameter("@BranchId", filter.BranchId, SqlDbType.Int),
                DbHelper.CreateParameter("@FromDate", filter.FromDate, SqlDbType.Date),
                DbHelper.CreateParameter("@ToDate", filter.ToDate, SqlDbType.Date),
                DbHelper.CreateParameter("@Search", filter.Search, SqlDbType.NVarChar, 200),
                DbHelper.CreateParameter("@CustomerId", filter.CustomerId, SqlDbType.Int),
                DbHelper.CreateParameter("@PageNumber", filter.PageNumber, SqlDbType.Int),
                DbHelper.CreateParameter("@PageSize", filter.PageSize, SqlDbType.Int)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_SalesEntryMaster_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<SalesMasterListModel>();
                    var properties = typeof(SalesMasterListModel).GetProperties().ToDictionary(p => p.Name.ToLower(), p => p);

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var item = new SalesMasterListModel();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i).ToLower();
                            if (reader.IsDBNull(i)) continue;

                            if (properties.TryGetValue(colName, out var prop) && prop.CanWrite)
                            {
                                var val = reader.GetValue(i);
                                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                prop.SetValue(item, Convert.ChangeType(val, targetType));
                            }
                        }
                        list.Add(item);
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            var pagedResult = new PagedListResult<SalesMasterListModel>
            {
                Items = result,
                TotalRecords = result.Count,
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return ApiResponse<PagedListResult<SalesMasterListModel>>.SuccessResult(pagedResult, "Data fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sales master list.");
            return ApiResponse<PagedListResult<SalesMasterListModel>>.FailureResult("Error fetching data.", ex.Message, null);
        }
    }

    public async Task<ApiResponse<List<SalesDetailListModel>>> GetAllSalesDetailAsync(int salesMasterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@SalesMasterId", salesMasterId, SqlDbType.Int)
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_SalesEntryDetail_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<SalesDetailListModel>();
                    var properties = typeof(SalesDetailListModel).GetProperties().ToDictionary(p => p.Name.ToLower(), p => p);

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var item = new SalesDetailListModel();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i).ToLower();
                            if (reader.IsDBNull(i)) continue;

                            if (properties.TryGetValue(colName, out var prop) && prop.CanWrite)
                            {
                                var val = reader.GetValue(i);
                                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                                prop.SetValue(item, Convert.ChangeType(val, targetType));
                            }
                        }
                        list.Add(item);
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<SalesDetailListModel>>.SuccessResult(result, "Details fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sales detail list.");
            return ApiResponse<List<SalesDetailListModel>>.FailureResult("Error fetching details.", ex.Message, null);
        }
    }
}
