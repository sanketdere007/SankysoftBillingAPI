using System.Data;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class ReportRepository : IReportRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<ReportRepository> _logger;

    public ReportRepository(DbHelper dbHelper, ILogger<ReportRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger;
    }

    public async Task<ApiResponse<PagedListResult<ProductWiseSalesReportModel>>> GetProductWiseSalesReportAsync(ProductWiseSalesReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@CompId", filter.CompId),
                new SqlParameter("@BranchId", filter.BranchId),
                new SqlParameter("@FromDate", filter.FromDate ?? (object)DBNull.Value),
                new SqlParameter("@ToDate", filter.ToDate ?? (object)DBNull.Value),
                new SqlParameter("@SearchText", string.IsNullOrWhiteSpace(filter.SearchText) ? (object)DBNull.Value : filter.SearchText),
                new SqlParameter("@PageNumber", filter.PageNumber),
                new SqlParameter("@PageSize", filter.PageSize),
                new SqlParameter("@SortColumn", filter.SortColumn),
                new SqlParameter("@SortDirection", filter.SortDirection)
            };

            var resultList = await _dbHelper.ExecuteStoredProcedureAsync(
                "SP_ProductWiseSalesReport",
                parameters,
                async reader =>
                {
                    var list = new List<ProductWiseSalesReportModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(new ProductWiseSalesReportModel
                        {
                            ProductId = reader["ProductId"] != DBNull.Value ? Convert.ToInt32(reader["ProductId"]) : 0,
                            ProductName = reader["ProductName"] != DBNull.Value ? reader["ProductName"].ToString() : null,
                            HSNCode = reader["HSNCode"] != DBNull.Value ? reader["HSNCode"].ToString() : null,
                            TotalQty = reader["TotalQty"] != DBNull.Value ? Convert.ToDecimal(reader["TotalQty"]) : 0,
                            TotalFreeQty = reader["TotalFreeQty"] != DBNull.Value ? Convert.ToDecimal(reader["TotalFreeQty"]) : 0,
                            TotalOverallQty = reader["TotalOverallQty"] != DBNull.Value ? Convert.ToDecimal(reader["TotalOverallQty"]) : 0,
                            TotalTaxableAmount = reader["TotalTaxableAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalTaxableAmount"]) : 0,
                            TotalTaxAmount = reader["TotalTaxAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalTaxAmount"]) : 0,
                            TotalAmount = reader["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalAmount"]) : 0,
                            TotalInvoices = reader["TotalInvoices"] != DBNull.Value ? Convert.ToInt32(reader["TotalInvoices"]) : 0,
                            TotalRecords = reader["TotalRecords"] != DBNull.Value ? Convert.ToInt32(reader["TotalRecords"]) : 0,
                            PageNumber = reader["PageNumber"] != DBNull.Value ? Convert.ToInt32(reader["PageNumber"]) : 1,
                            PageSize = reader["PageSize"] != DBNull.Value ? Convert.ToInt32(reader["PageSize"]) : 20
                        });
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            var totalRecords = resultList.FirstOrDefault()?.TotalRecords ?? 0;
            var totalPages = totalRecords > 0 ? (int)Math.Ceiling(totalRecords / (double)filter.PageSize) : 0;

            var pagedResult = new PagedListResult<ProductWiseSalesReportModel>
            {
                Items = resultList,
                TotalRecords = totalRecords,
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages
            };

            return ApiResponse<PagedListResult<ProductWiseSalesReportModel>>.SuccessResult(pagedResult, "Report fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetProductWiseSalesReportAsync");
            return ApiResponse<PagedListResult<ProductWiseSalesReportModel>>.FailureResult(
                message: "Error fetching report", 
                error: ex.Message,
                data: new PagedListResult<ProductWiseSalesReportModel>());
        }
    }

    public async Task<ApiResponse<PagedListResult<ProductWiseCustomerPurchaseListModel>>> GetProductWiseCustomerPurchaseListAsync(ProductWiseCustomerPurchaseListFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@ProductId", filter.ProductId),
                new SqlParameter("@FromDate", filter.FromDate ?? (object)DBNull.Value),
                new SqlParameter("@ToDate", filter.ToDate ?? (object)DBNull.Value),
                new SqlParameter("@CompId", filter.CompId ?? (object)DBNull.Value),
                new SqlParameter("@BranchId", filter.BranchId ?? (object)DBNull.Value),
                new SqlParameter("@SearchText", string.IsNullOrWhiteSpace(filter.SearchText) ? (object)DBNull.Value : filter.SearchText),
                new SqlParameter("@PageNumber", filter.PageNumber),
                new SqlParameter("@PageSize", filter.PageSize)
            };

            var resultList = await _dbHelper.ExecuteStoredProcedureAsync(
                "SP_ProductWise_CustomerPurchaseList",
                parameters,
                async reader =>
                {
                    var list = new List<ProductWiseCustomerPurchaseListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(new ProductWiseCustomerPurchaseListModel
                        {
                            CustomerId = reader["CustomerId"] != DBNull.Value ? Convert.ToInt32(reader["CustomerId"]) : null,
                            CustomerName = reader["CustomerName"] != DBNull.Value ? reader["CustomerName"].ToString() : null,
                            CustomerMobile = reader["CustomerMobile"] != DBNull.Value ? reader["CustomerMobile"].ToString() : null,
                            PurchaseDate = reader["PurchaseDate"] != DBNull.Value ? Convert.ToDateTime(reader["PurchaseDate"]) : null,
                            SalesMasterId = reader["SalesMasterId"] != DBNull.Value ? Convert.ToInt32(reader["SalesMasterId"]) : 0,
                            Qty = reader["Qty"] != DBNull.Value ? Convert.ToDecimal(reader["Qty"]) : 0,
                            FreeQty = reader["FreeQty"] != DBNull.Value ? Convert.ToDecimal(reader["FreeQty"]) : 0,
                            TotalQty = reader["TotalQty"] != DBNull.Value ? Convert.ToDecimal(reader["TotalQty"]) : 0,
                            Rate = reader["Rate"] != DBNull.Value ? Convert.ToDecimal(reader["Rate"]) : 0,
                            Amount = reader["Amount"] != DBNull.Value ? Convert.ToDecimal(reader["Amount"]) : 0,
                            TotalRecords = reader["TotalRecords"] != DBNull.Value ? Convert.ToInt32(reader["TotalRecords"]) : 0,
                            PageNumber = reader["PageNumber"] != DBNull.Value ? Convert.ToInt32(reader["PageNumber"]) : 1,
                            PageSize = reader["PageSize"] != DBNull.Value ? Convert.ToInt32(reader["PageSize"]) : 20
                        });
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            var totalRecords = resultList.FirstOrDefault()?.TotalRecords ?? 0;
            var totalPages = totalRecords > 0 ? (int)Math.Ceiling(totalRecords / (double)filter.PageSize) : 0;

            var pagedResult = new PagedListResult<ProductWiseCustomerPurchaseListModel>
            {
                Items = resultList,
                TotalRecords = totalRecords,
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = totalPages
            };

            return ApiResponse<PagedListResult<ProductWiseCustomerPurchaseListModel>>.SuccessResult(pagedResult, "Report fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetProductWiseCustomerPurchaseListAsync");
            return ApiResponse<PagedListResult<ProductWiseCustomerPurchaseListModel>>.FailureResult(
                message: "Error fetching report", 
                error: ex.Message,
                data: new PagedListResult<ProductWiseCustomerPurchaseListModel>());
        }
    }
}
