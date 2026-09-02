using System.Data;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class SalesPendingAmountRepository : ISalesPendingAmountRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<SalesPendingAmountRepository> _logger;

    public SalesPendingAmountRepository(DbHelper dbHelper, ILogger<SalesPendingAmountRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<PagedListResult<SalesPendingAmountModel>>> GetPendingAmountAsync(
        SalesPendingAmountFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            filter ??= new SalesPendingAmountFilterDto();

            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 20 : filter.PageSize;

            var parameters = new[]
            {
                DbHelper.CreateParameter("@CompId", ToNullableInt(filter.CompId), SqlDbType.Int),
                DbHelper.CreateParameter("@BranchId", ToNullableInt(filter.BranchId), SqlDbType.Int),
                DbHelper.CreateParameter("@CustomerId", ToNullableInt(filter.CustomerId), SqlDbType.Int),
                DbHelper.CreateParameter("@Search", string.IsNullOrWhiteSpace(filter.Search) ? (object)DBNull.Value : filter.Search.Trim(), SqlDbType.NVarChar, 200),
                DbHelper.CreateParameter("@PageNumber", pageNumber, SqlDbType.Int),
                DbHelper.CreateParameter("@PageSize", pageSize, SqlDbType.Int)
            };

            var pagedResult = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_SalesEntry_PendingAmount_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var result = new PagedListResult<SalesPendingAmountModel>
                    {
                        CurrentPage = pageNumber,
                        PageSize = pageSize
                    };

                    if (IsProcedureErrorResult(reader))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            throw new InvalidOperationException(ReadProcedureErrorMessage(reader));
                        }
                    }

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        if (result.Items.Count == 0)
                        {
                            result.TotalRecords = ReadInt(reader, "TotalRecords");
                            result.TotalPages = ReadInt(reader, "TotalPages");
                            result.CurrentPage = ReadInt(reader, "CurrentPage", pageNumber);
                            result.PageSize = ReadInt(reader, "PageSize", pageSize);
                        }

                        result.Items.Add(MapPendingAmountFromReader(reader));
                    }

                    return result;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<PagedListResult<SalesPendingAmountModel>>.SuccessResult(
                data: pagedResult,
                message: $"Successfully retrieved {pagedResult.Items.Count} pending invoice(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching pending sales amounts.");
            return ApiResponse<PagedListResult<SalesPendingAmountModel>>.FailureResult(
                message: "Unable to retrieve pending sales amount list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching pending sales amounts.");
            return ApiResponse<PagedListResult<SalesPendingAmountModel>>.FailureResult(
                message: "An unexpected error occurred while fetching pending sales amounts.",
                error: ex.Message);
        }
    }

    private static SalesPendingAmountModel MapPendingAmountFromReader(SqlDataReader reader)
    {
        return new SalesPendingAmountModel
        {
            SalesMaster_Id = ReadInt(reader, "SalesMaster_Id"),
            SalesMaster_InvoiceNo = ReadString(reader, "SalesMaster_InvoiceNo"),
            SalesMaster_InvoiceDate = ReadDateTime(reader, "SalesMaster_InvoiceDate"),
            SalesMaster_LedgerId = ReadNullableInt(reader, "SalesMaster_LedgerId"),
            AccLedger_Name = ReadString(reader, "AccLedger_Name"),
            SalesMaster_GrandTotal = ReadDecimal(reader, "SalesMaster_GrandTotal"),
            SalesMaster_PaidAmount = ReadDecimal(reader, "SalesMaster_PaidAmount"),
            SalesMaster_BalanceAmount = ReadDecimal(reader, "SalesMaster_BalanceAmount"),
            Cust_Code = ReadString(reader, "Cust_Code"),
            Cust_Name = ReadString(reader, "Cust_Name"),
            Cust_MobileNo = ReadString(reader, "Cust_MobileNo")
        };
    }

    private static object ToNullableInt(int? value)
        => value.HasValue && value.Value > 0 ? value.Value : DBNull.Value;

    private static bool IsProcedureErrorResult(SqlDataReader reader)
        => HasColumn(reader, "Success") && HasColumn(reader, "ErrorNumber") && !HasColumn(reader, "SalesMaster_Id");

    private static string ReadProcedureErrorMessage(SqlDataReader reader)
        => ReadString(reader, "Message") ?? "Stored procedure failed.";

    private static int ReadInt(SqlDataReader reader, string columnName, int defaultValue = 0)
    {
        if (!HasColumn(reader, columnName) || reader.IsDBNull(reader.GetOrdinal(columnName)))
            return defaultValue;

        return Convert.ToInt32(reader[columnName]);
    }

    private static int? ReadNullableInt(SqlDataReader reader, string columnName)
    {
        if (!HasColumn(reader, columnName) || reader.IsDBNull(reader.GetOrdinal(columnName)))
            return null;

        return Convert.ToInt32(reader[columnName]);
    }

    private static decimal ReadDecimal(SqlDataReader reader, string columnName)
    {
        if (!HasColumn(reader, columnName) || reader.IsDBNull(reader.GetOrdinal(columnName)))
            return 0m;

        return Convert.ToDecimal(reader[columnName]);
    }

    private static string? ReadString(SqlDataReader reader, string columnName)
    {
        if (!HasColumn(reader, columnName) || reader.IsDBNull(reader.GetOrdinal(columnName)))
            return null;

        return Convert.ToString(reader[columnName]);
    }

    private static DateTime? ReadDateTime(SqlDataReader reader, string columnName)
    {
        if (!HasColumn(reader, columnName) || reader.IsDBNull(reader.GetOrdinal(columnName)))
            return null;

        return Convert.ToDateTime(reader[columnName]);
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
