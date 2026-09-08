using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Billing_Software_Api.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<DashboardRepository> _logger;

    public DashboardRepository(DbHelper dbHelper, ILogger<DashboardRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<DashboardSummaryResponse>> GetDashboardSummaryAsync(DashboardSummaryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new List<SqlParameter>
            {
                DbHelper.CreateParameter("@CompId", request.CompId, SqlDbType.Int),
                DbHelper.CreateParameter("@BranchId", request.BranchId, SqlDbType.Int),
                DbHelper.CreateParameter("@FromDate", request.FromDate, SqlDbType.Date),
                DbHelper.CreateParameter("@ToDate", request.ToDate, SqlDbType.Date),
                DbHelper.CreateParameter("@LowStockQty", request.LowStockQty, SqlDbType.Decimal)
            };

            var data = await _dbHelper.ExecuteStoredProcedureAsync(
                "dbo.SP_Dashboard_GetSummary",
                parameters,
                async reader =>
                {
                    var result = new DashboardSummaryResponse();
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        result.LowStockProducts = reader["LowStockProducts"] != DBNull.Value ? Convert.ToInt32(reader["LowStockProducts"]) : 0;
                        result.TotalSales = reader["TotalSales"] != DBNull.Value ? Convert.ToDecimal(reader["TotalSales"]) : 0m;
                        result.TotalPurchase = reader["TotalPurchase"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPurchase"]) : 0m;
                        result.OutOfStockProducts = reader["OutOfStockProducts"] != DBNull.Value ? Convert.ToInt32(reader["OutOfStockProducts"]) : 0;
                        result.TotalProducts = reader["TotalProducts"] != DBNull.Value ? Convert.ToInt32(reader["TotalProducts"]) : 0;
                        result.PendingAmount = reader["PendingAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PendingAmount"]) : 0m;
                        result.Collection = reader["Collection"] != DBNull.Value ? Convert.ToDecimal(reader["Collection"]) : 0m;
                        result.TotalSalesOrders = reader["TotalSalesOrders"] != DBNull.Value ? Convert.ToInt32(reader["TotalSalesOrders"]) : 0;
                        
                        if (reader["FromDate"] != DBNull.Value)
                            result.FromDate = Convert.ToDateTime(reader["FromDate"]);
                        
                        if (reader["ToDate"] != DBNull.Value)
                            result.ToDate = Convert.ToDateTime(reader["ToDate"]);
                    }
                    return result;
                },
                cancellationToken: cancellationToken
            );

            return ApiResponse<DashboardSummaryResponse>.SuccessResult(data, "Dashboard summary retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while retrieving dashboard summary.");
            return ApiResponse<DashboardSummaryResponse>.FailureResult(
                "A database error occurred while retrieving the dashboard summary.",
                sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving dashboard summary.");
            return ApiResponse<DashboardSummaryResponse>.FailureResult(
                "An unexpected error occurred while retrieving the dashboard summary.",
                ex.Message);
        }
    }
}
