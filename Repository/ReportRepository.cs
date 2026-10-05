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

    public async Task<ApiResponse<OutstandingReceivableReportModel>> GetOutstandingReceivableReportAsync(OutstandingReceivableReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@CompId", filter.CompId),
                new SqlParameter("@BranchId", filter.BranchId),
                new SqlParameter("@FromDate", filter.FromDate ?? (object)DBNull.Value),
                new SqlParameter("@ToDate", filter.ToDate ?? (object)DBNull.Value),
                new SqlParameter("@Search", string.IsNullOrWhiteSpace(filter.Search) ? (object)DBNull.Value : filter.Search),
                new SqlParameter("@CustomerId", filter.CustomerId ?? (object)DBNull.Value),
                new SqlParameter("@RouteId", filter.RouteId ?? (object)DBNull.Value),
                new SqlParameter("@AreaId", filter.AreaId ?? (object)DBNull.Value),
                new SqlParameter("@CityId", filter.CityId ?? (object)DBNull.Value),
                new SqlParameter("@StateId", filter.StateId ?? (object)DBNull.Value),
                new SqlParameter("@OnlyOutstanding", filter.OnlyOutstanding)
            };

            var reportModel = await _dbHelper.ExecuteStoredProcedureAsync(
                "SP_OutstandingReceivableReport",
                parameters,
                async reader =>
                {
                    var result = new OutstandingReceivableReportModel();

                    // Result 1: Invoice Detail
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        result.InvoiceDetails.Add(new OutstandingInvoiceDetailModel
                        {
                            Cust_Id = reader["Cust_Id"] != DBNull.Value ? Convert.ToInt32(reader["Cust_Id"]) : 0,
                            Cust_Code = reader["Cust_Code"] != DBNull.Value ? reader["Cust_Code"].ToString() : null,
                            Cust_Name = reader["Cust_Name"] != DBNull.Value ? reader["Cust_Name"].ToString() : null,
                            Cust_MobileNo = reader["Cust_MobileNo"] != DBNull.Value ? reader["Cust_MobileNo"].ToString() : null,
                            Cust_StateId = reader["Cust_StateId"] != DBNull.Value ? Convert.ToInt32(reader["Cust_StateId"]) : null,
                            State_Name = reader["State_Name"] != DBNull.Value ? reader["State_Name"].ToString() : null,
                            Cust_CityId = reader["Cust_CityId"] != DBNull.Value ? Convert.ToInt32(reader["Cust_CityId"]) : null,
                            City_Name = reader["City_Name"] != DBNull.Value ? reader["City_Name"].ToString() : null,
                            Cust_AreaId = reader["Cust_AreaId"] != DBNull.Value ? Convert.ToInt32(reader["Cust_AreaId"]) : null,
                            Area_Name = reader["Area_Name"] != DBNull.Value ? reader["Area_Name"].ToString() : null,
                            Cust_RouteId = reader["Cust_RouteId"] != DBNull.Value ? Convert.ToInt32(reader["Cust_RouteId"]) : null,
                            Route_Name = reader["Route_Name"] != DBNull.Value ? reader["Route_Name"].ToString() : null,
                            SalesMaster_Id = reader["SalesMaster_Id"] != DBNull.Value ? Convert.ToInt32(reader["SalesMaster_Id"]) : 0,
                            SalesMaster_InvoiceNo = reader["SalesMaster_InvoiceNo"] != DBNull.Value ? reader["SalesMaster_InvoiceNo"].ToString() : null,
                            SalesMaster_InvoiceDate = reader["SalesMaster_InvoiceDate"] != DBNull.Value ? Convert.ToDateTime(reader["SalesMaster_InvoiceDate"]) : null,
                            BillAmount = reader["BillAmount"] != DBNull.Value ? Convert.ToDecimal(reader["BillAmount"]) : 0,
                            PaidAmount = reader["PaidAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaidAmount"]) : 0,
                            BalanceAmount = reader["BalanceAmount"] != DBNull.Value ? Convert.ToDecimal(reader["BalanceAmount"]) : 0,
                            DaysOutstanding = reader["DaysOutstanding"] != DBNull.Value ? Convert.ToInt32(reader["DaysOutstanding"]) : 0,
                            RowType = reader["RowType"] != DBNull.Value ? reader["RowType"].ToString() : null
                        });
                    }

                    // Result 2: Party Total
                    if (await reader.NextResultAsync(cancellationToken))
                    {
                        while (await reader.ReadAsync(cancellationToken))
                        {
                            result.PartyTotals.Add(new OutstandingPartyTotalModel
                            {
                                Cust_Id = reader["Cust_Id"] != DBNull.Value ? Convert.ToInt32(reader["Cust_Id"]) : 0,
                                Cust_Code = reader["Cust_Code"] != DBNull.Value ? reader["Cust_Code"].ToString() : null,
                                Cust_Name = reader["Cust_Name"] != DBNull.Value ? reader["Cust_Name"].ToString() : null,
                                TotalInvoices = reader["TotalInvoices"] != DBNull.Value ? Convert.ToInt32(reader["TotalInvoices"]) : 0,
                                TotalBillAmount = reader["TotalBillAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalBillAmount"]) : 0,
                                TotalPaidAmount = reader["TotalPaidAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalPaidAmount"]) : 0,
                                TotalBalanceAmount = reader["TotalBalanceAmount"] != DBNull.Value ? Convert.ToDecimal(reader["TotalBalanceAmount"]) : 0,
                                RowType = reader["RowType"] != DBNull.Value ? reader["RowType"].ToString() : null
                            });
                        }
                    }

                    // Result 3: Grand Total
                    if (await reader.NextResultAsync(cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            result.GrandTotal = new OutstandingGrandTotalModel
                            {
                                TotalInvoices = reader["TotalInvoices"] != DBNull.Value ? Convert.ToInt32(reader["TotalInvoices"]) : 0,
                                TotalCustomers = reader["TotalCustomers"] != DBNull.Value ? Convert.ToInt32(reader["TotalCustomers"]) : 0,
                                GrandTotalBillAmount = reader["GrandTotalBillAmount"] != DBNull.Value ? Convert.ToDecimal(reader["GrandTotalBillAmount"]) : 0,
                                GrandTotalPaidAmount = reader["GrandTotalPaidAmount"] != DBNull.Value ? Convert.ToDecimal(reader["GrandTotalPaidAmount"]) : 0,
                                GrandTotalBalanceAmount = reader["GrandTotalBalanceAmount"] != DBNull.Value ? Convert.ToDecimal(reader["GrandTotalBalanceAmount"]) : 0
                            };
                        }
                    }

                    return result;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<OutstandingReceivableReportModel>.SuccessResult(reportModel, "Outstanding Receivable Report fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOutstandingReceivableReportAsync");
            return ApiResponse<OutstandingReceivableReportModel>.FailureResult(
                message: "Error fetching report", 
                error: ex.Message,
                data: new OutstandingReceivableReportModel());
        }
    }
}
