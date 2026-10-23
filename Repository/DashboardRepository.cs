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

    public async Task<ApiResponse<DashboardChartsResponse>> GetDashboardChartsAsync(DashboardChartsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request.FromDate.HasValue && request.ToDate.HasValue && request.FromDate.Value.Date > request.ToDate.Value.Date)
            {
                return ApiResponse<DashboardChartsResponse>.FailureResult(
                    "FromDate cannot be greater than ToDate.",
                    "FromDate cannot be greater than ToDate.");
            }

            var parameters = new List<SqlParameter>
            {
                DbHelper.CreateParameter("@CompId", request.CompId.HasValue && request.CompId.Value > 0 ? request.CompId.Value : 1, SqlDbType.Int),
                DbHelper.CreateParameter("@BranchId", request.BranchId.HasValue && request.BranchId.Value > 0 ? request.BranchId.Value : 1, SqlDbType.Int),
                DbHelper.CreateParameter("@FromDate", request.FromDate.HasValue ? request.FromDate.Value.Date : (object?)null, SqlDbType.Date),
                DbHelper.CreateParameter("@ToDate", request.ToDate.HasValue ? request.ToDate.Value.Date : (object?)null, SqlDbType.Date),
                DbHelper.CreateParameter("@TopProducts", request.TopProducts.HasValue && request.TopProducts.Value > 0 ? request.TopProducts.Value : 5, SqlDbType.Int)
            };

            var data = await _dbHelper.ExecuteStoredProcedureAsync(
                "dbo.SP_Dashboard_GetCharts",
                parameters,
                async reader =>
                {
                    var result = new DashboardChartsResponse();

                    // 1. DAILY SALES TREND
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        result.DailySalesTrend.Add(new DailySalesTrendItem
                        {
                            SalesDate = reader["SalesDate"] != DBNull.Value ? Convert.ToDateTime(reader["SalesDate"]) : DateTime.MinValue,
                            DayName = reader["DayName"] != DBNull.Value ? Convert.ToString(reader["DayName"]) ?? string.Empty : string.Empty,
                            TotalBills = reader["TotalBills"] != DBNull.Value ? Convert.ToInt64(reader["TotalBills"]) : 0,
                            SalesAmount = reader["SalesAmount"] != DBNull.Value ? Convert.ToDecimal(reader["SalesAmount"]) : 0m
                        });
                    }

                    // 2. PAYMENT COLLECTION ANALYSIS
                    if (await reader.NextResultAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            result.PaymentCollection.Add(new PaymentCollectionItem
                            {
                                CollectionType = reader["CollectionType"] != DBNull.Value ? Convert.ToString(reader["CollectionType"]) ?? string.Empty : string.Empty,
                                Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0m,
                                Percentage = reader["Percentage"] != DBNull.Value ? Convert.ToDecimal(reader["Percentage"]) : 0m
                            });
                        }
                    }

                    // 3. MONTHLY SALES VS PURCHASE
                    if (await reader.NextResultAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            result.MonthlySalesVsPurchase.Add(new MonthlySalesVsPurchaseItem
                            {
                                MonthStart = reader["MonthStart"] != DBNull.Value ? Convert.ToDateTime(reader["MonthStart"]) : DateTime.MinValue,
                                MonthName = reader["MonthName"] != DBNull.Value ? Convert.ToString(reader["MonthName"]) ?? string.Empty : string.Empty,
                                Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                                SalesAmount = reader["SalesAmount"] != DBNull.Value ? Convert.ToDecimal(reader["SalesAmount"]) : 0m,
                                PurchaseAmount = reader["PurchaseAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PurchaseAmount"]) : 0m
                            });
                        }
                    }

                    // 4. TOP SELLING PRODUCTS
                    if (await reader.NextResultAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            result.TopSellingProducts.Add(new TopSellingProductItem
                            {
                                ProductId = reader["ProductId"] != DBNull.Value ? Convert.ToInt32(reader["ProductId"]) : 0,
                                ProductName = reader["ProductName"] != DBNull.Value ? Convert.ToString(reader["ProductName"]) ?? string.Empty : string.Empty,
                                TotalQty = reader["TotalQty"] != DBNull.Value ? Convert.ToDecimal(reader["TotalQty"]) : 0m,
                                TotalAmount = reader["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalAmount"]) : 0m,
                                TotalBills = reader["TotalBills"] != DBNull.Value ? Convert.ToInt32(reader["TotalBills"]) : 0
                            });
                        }
                    }

                    return result;
                },
                commandTimeout: 60,
                cancellationToken: cancellationToken
            );

            return ApiResponse<DashboardChartsResponse>.SuccessResult(data, "Dashboard charts retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while retrieving dashboard charts.");
            return ApiResponse<DashboardChartsResponse>.FailureResult(
                sqlEx.Message,
                sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving dashboard charts.");
            return ApiResponse<DashboardChartsResponse>.FailureResult(
                "An unexpected error occurred while retrieving dashboard charts.",
                ex.Message);
        }
    }
}
