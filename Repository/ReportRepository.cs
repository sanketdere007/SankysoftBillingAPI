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
                            BillWiseDiscountPerecentage = reader["BillWiseDiscountPerecentage"] != DBNull.Value ? Convert.ToDecimal(reader["BillWiseDiscountPerecentage"]) : 0,
                            BillWiseDiscountAmount = reader["BillWiseDiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["BillWiseDiscountAmount"]) : 0,
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

    public async Task<ApiResponse<PagedListResult<PaymentEntryReportModel>>> GetPaymentEntryReportAsync(PaymentEntryReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                new SqlParameter("@CompId", filter.CompId),
                new SqlParameter("@BranchId", filter.BranchId),
                new SqlParameter("@FromDate", filter.FromDate ?? (object)DBNull.Value),
                new SqlParameter("@ToDate", filter.ToDate ?? (object)DBNull.Value),
                new SqlParameter("@PaymentMaster_Id", filter.PaymentMaster_Id ?? (object)DBNull.Value),
                new SqlParameter("@PaymentNo", string.IsNullOrWhiteSpace(filter.PaymentNo) ? (object)DBNull.Value : filter.PaymentNo),
                new SqlParameter("@InvoiceNo", string.IsNullOrWhiteSpace(filter.InvoiceNo) ? (object)DBNull.Value : filter.InvoiceNo),
                new SqlParameter("@Search", string.IsNullOrWhiteSpace(filter.Search) ? (object)DBNull.Value : filter.Search),
                new SqlParameter("@PaymentType", string.IsNullOrWhiteSpace(filter.PaymentType) ? (object)DBNull.Value : filter.PaymentType),
                new SqlParameter("@Status", string.IsNullOrWhiteSpace(filter.Status) ? (object)DBNull.Value : filter.Status),
                new SqlParameter("@Page", filter.Page),
                new SqlParameter("@PageSize", filter.PageSize)
            };

            var resultList = await _dbHelper.ExecuteStoredProcedureAsync(
                "SP_PaymentEntry_Report",
                parameters,
                async reader =>
                {
                    var list = new List<PaymentEntryReportModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(new PaymentEntryReportModel
                        {
                            PaymentMaster_Id = reader["PaymentMaster_Id"] != DBNull.Value ? Convert.ToInt32(reader["PaymentMaster_Id"]) : 0,
                            PaymentMaster_PaymentNo = reader["PaymentMaster_PaymentNo"] != DBNull.Value ? reader["PaymentMaster_PaymentNo"].ToString() : null,
                            PaymentMaster_PaymentDate = reader["PaymentMaster_PaymentDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentMaster_PaymentDate"]) : null,
                            PaymentMaster_Type = reader["PaymentMaster_Type"] != DBNull.Value ? reader["PaymentMaster_Type"].ToString() : null,
                            PaymentMaster_Status = reader["PaymentMaster_Status"] != DBNull.Value ? reader["PaymentMaster_Status"].ToString() : null,
                            PaymentMaster_InvoiceId = reader["PaymentMaster_InvoiceId"] != DBNull.Value ? Convert.ToInt32(reader["PaymentMaster_InvoiceId"]) : null,
                            PaymentMaster_InvoiceNo = reader["PaymentMaster_InvoiceNo"] != DBNull.Value ? reader["PaymentMaster_InvoiceNo"].ToString() : null,
                            PaymentMaster_InvoiceDate = reader["PaymentMaster_InvoiceDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentMaster_InvoiceDate"]) : null,
                            PaymentMaster_AccountId = reader["PaymentMaster_AccountId"] != DBNull.Value ? Convert.ToInt32(reader["PaymentMaster_AccountId"]) : null,
                            PaymentMaster_TotalAmount = reader["PaymentMaster_TotalAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaymentMaster_TotalAmount"]) : 0,
                            PaymentMaster_CashAmount = reader["PaymentMaster_CashAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaymentMaster_CashAmount"]) : 0,
                            PaymentMaster_UPIAmount = reader["PaymentMaster_UPIAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaymentMaster_UPIAmount"]) : 0,
                            PaymentMaster_ChequeAmount = reader["PaymentMaster_ChequeAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaymentMaster_ChequeAmount"]) : 0,
                            PaymentMaster_BankAmount = reader["PaymentMaster_BankAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaymentMaster_BankAmount"]) : 0,
                            PaymentMaster_CardAmount = reader["PaymentMaster_CardAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaymentMaster_CardAmount"]) : 0,
                            PaymentMaster_OtherAmount = reader["PaymentMaster_OtherAmount"] != DBNull.Value ? Convert.ToDecimal(reader["PaymentMaster_OtherAmount"]) : 0,
                            PaymentModes = reader["PaymentModes"] != DBNull.Value ? reader["PaymentModes"].ToString() : null,
                            PaymentMaster_CashRemark = reader["PaymentMaster_CashRemark"] != DBNull.Value ? reader["PaymentMaster_CashRemark"].ToString() : null,
                            PaymentMaster_UPITransactionNo = reader["PaymentMaster_UPITransactionNo"] != DBNull.Value ? reader["PaymentMaster_UPITransactionNo"].ToString() : null,
                            PaymentMaster_UPIReferenceNo = reader["PaymentMaster_UPIReferenceNo"] != DBNull.Value ? reader["PaymentMaster_UPIReferenceNo"].ToString() : null,
                            PaymentMaster_ChequeNo = reader["PaymentMaster_ChequeNo"] != DBNull.Value ? reader["PaymentMaster_ChequeNo"].ToString() : null,
                            PaymentMaster_ChequeDate = reader["PaymentMaster_ChequeDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentMaster_ChequeDate"]) : null,
                            PaymentMaster_ChequeBankName = reader["PaymentMaster_ChequeBankName"] != DBNull.Value ? reader["PaymentMaster_ChequeBankName"].ToString() : null,
                            PaymentMaster_ChequeBranchName = reader["PaymentMaster_ChequeBranchName"] != DBNull.Value ? reader["PaymentMaster_ChequeBranchName"].ToString() : null,
                            PaymentMaster_BankTransferType = reader["PaymentMaster_BankTransferType"] != DBNull.Value ? reader["PaymentMaster_BankTransferType"].ToString() : null,
                            PaymentMaster_BankName = reader["PaymentMaster_BankName"] != DBNull.Value ? reader["PaymentMaster_BankName"].ToString() : null,
                            PaymentMaster_BankAccountNo = reader["PaymentMaster_BankAccountNo"] != DBNull.Value ? reader["PaymentMaster_BankAccountNo"].ToString() : null,
                            PaymentMaster_BankTransactionNo = reader["PaymentMaster_BankTransactionNo"] != DBNull.Value ? reader["PaymentMaster_BankTransactionNo"].ToString() : null,
                            PaymentMaster_BankReferenceNo = reader["PaymentMaster_BankReferenceNo"] != DBNull.Value ? reader["PaymentMaster_BankReferenceNo"].ToString() : null,
                            PaymentMaster_BankDate = reader["PaymentMaster_BankDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentMaster_BankDate"]) : null,
                            PaymentMaster_Remark = reader["PaymentMaster_Remark"] != DBNull.Value ? reader["PaymentMaster_Remark"].ToString() : null,
                            PaymentMaster_CreatedBy = reader["PaymentMaster_CreatedBy"] != DBNull.Value ? Convert.ToInt32(reader["PaymentMaster_CreatedBy"]) : null,
                            PaymentMaster_CreatedDate = reader["PaymentMaster_CreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentMaster_CreatedDate"]) : null,
                            PaymentMaster_ModifiedBy = reader["PaymentMaster_ModifiedBy"] != DBNull.Value ? Convert.ToInt32(reader["PaymentMaster_ModifiedBy"]) : null,
                            PaymentMaster_ModifiedDate = reader["PaymentMaster_ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["PaymentMaster_ModifiedDate"]) : null
                        });
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            var pagedResult = new PagedListResult<PaymentEntryReportModel>
            {
                Items = resultList,
                TotalRecords = 0, // Not returned by this SP
                CurrentPage = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = 0
            };

            return ApiResponse<PagedListResult<PaymentEntryReportModel>>.SuccessResult(pagedResult, "Report fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPaymentEntryReportAsync");
            return ApiResponse<PagedListResult<PaymentEntryReportModel>>.FailureResult(
                message: "Error fetching report", 
                error: ex.Message,
                data: new PagedListResult<PaymentEntryReportModel>());
        }
    }

    public async Task<ApiResponse<PagedListResult<BatchWiseStockReportModel>>> GetBatchWiseStockReportAsync(BatchWiseStockReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 500 : filter.PageSize;

            var parameters = new[]
            {
                new SqlParameter("@CompId", filter.CompId.HasValue ? filter.CompId.Value : DBNull.Value),
                new SqlParameter("@BranchId", filter.BranchId.HasValue ? filter.BranchId.Value : DBNull.Value),
                new SqlParameter("@ProductId", filter.ProductId.HasValue ? filter.ProductId.Value : DBNull.Value),
                new SqlParameter("@Search", string.IsNullOrWhiteSpace(filter.Search) ? (object)DBNull.Value : filter.Search.Trim()),
                new SqlParameter("@OnlyInStock", filter.OnlyInStock),
                new SqlParameter("@PageNumber", pageNumber),
                new SqlParameter("@PageSize", pageSize)
            };

            var resultList = await _dbHelper.ExecuteStoredProcedureAsync(
                "dbo.SP_BatchWiseStockReport",
                parameters,
                async reader =>
                {
                    var list = new List<BatchWiseStockReportModel>();
                    bool? hasTotalRecordsCol = null;

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        if (!hasTotalRecordsCol.HasValue)
                        {
                            hasTotalRecordsCol = HasColumn(reader, "TotalRecords");
                        }

                        var item = new BatchWiseStockReportModel
                        {
                            Batch_Id = reader["Batch_Id"] != DBNull.Value ? Convert.ToInt32(reader["Batch_Id"]) : 0,
                            Batch_CompId = reader["Batch_CompId"] != DBNull.Value ? Convert.ToInt32(reader["Batch_CompId"]) : null,
                            Batch_BranchId = reader["Batch_BranchId"] != DBNull.Value ? Convert.ToInt32(reader["Batch_BranchId"]) : null,
                            Batch_ProductId = reader["Batch_ProductId"] != DBNull.Value ? Convert.ToInt32(reader["Batch_ProductId"]) : null,
                            Prod_Name = reader["Prod_Name"] != DBNull.Value ? reader["Prod_Name"].ToString() : null,
                            Prod_UnitValue = reader["Prod_UnitValue"] != DBNull.Value ? Convert.ToDecimal(reader["Prod_UnitValue"]) : null,
                            Unit_ShortName = reader["Unit_ShortName"] != DBNull.Value ? reader["Unit_ShortName"].ToString() : null,
                            Batch_Barcode = reader["Batch_Barcode"] != DBNull.Value ? reader["Batch_Barcode"].ToString() : null,
                            Batch_EANCode = reader["Batch_EANCode"] != DBNull.Value ? reader["Batch_EANCode"].ToString() : null,
                            Batch_Number = reader["Batch_Number"] != DBNull.Value ? reader["Batch_Number"].ToString() : null,
                            Batch_Stock = reader["Batch_Stock"] != DBNull.Value ? Convert.ToDecimal(reader["Batch_Stock"]) : null,
                            Batch_AvailableStock = reader["Batch_AvailableStock"] != DBNull.Value ? Convert.ToDecimal(reader["Batch_AvailableStock"]) : null,
                            Batch_LandingPrice = reader["Batch_LandingPrice"] != DBNull.Value ? Convert.ToDecimal(reader["Batch_LandingPrice"]) : null,
                            Batch_PurchasePrice = reader["Batch_PurchasePrice"] != DBNull.Value ? Convert.ToDecimal(reader["Batch_PurchasePrice"]) : null,
                            Batch_MRP = reader["Batch_MRP"] != DBNull.Value ? Convert.ToDecimal(reader["Batch_MRP"]) : null,
                            Batch_SellingPrice = reader["Batch_SellingPrice"] != DBNull.Value ? Convert.ToDecimal(reader["Batch_SellingPrice"]) : null,
                            TotalRecords = (hasTotalRecordsCol == true && reader["TotalRecords"] != DBNull.Value) ? Convert.ToInt32(reader["TotalRecords"]) : 0
                        };

                        list.Add(item);
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            var totalRecords = resultList.FirstOrDefault()?.TotalRecords ?? 0;
            if (totalRecords == 0)
            {
                totalRecords = resultList.Count;
            }

            var totalPages = pageSize > 0 && totalRecords > 0 
                ? (int)Math.Ceiling(totalRecords / (double)pageSize) 
                : 0;

            var pagedResult = new PagedListResult<BatchWiseStockReportModel>
            {
                Items = resultList,
                TotalRecords = totalRecords,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return ApiResponse<PagedListResult<BatchWiseStockReportModel>>.SuccessResult(pagedResult, "Batch-wise stock report fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetBatchWiseStockReportAsync");
            return ApiResponse<PagedListResult<BatchWiseStockReportModel>>.FailureResult(
                message: "Error fetching batch-wise stock report", 
                error: ex.Message,
                data: new PagedListResult<BatchWiseStockReportModel>());
        }
    }

    public async Task<ApiResponse<PagedListResult<CustomerWiseSalesReportModel>>> GetCustomerWiseSalesReportAsync(CustomerWiseSalesReportFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            if (filter.FromDate.HasValue && filter.ToDate.HasValue && filter.FromDate.Value.Date > filter.ToDate.Value.Date)
            {
                return ApiResponse<PagedListResult<CustomerWiseSalesReportModel>>.FailureResult(
                    message: "FromDate cannot be greater than ToDate.",
                    error: "FromDate cannot be greater than ToDate.",
                    data: new PagedListResult<CustomerWiseSalesReportModel>());
            }

            var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var pageSize = filter.PageSize < 1 ? 20 : (filter.PageSize > 500 ? 500 : filter.PageSize);

            var parameters = new[]
            {
                new SqlParameter("@CompId", filter.CompId.HasValue ? filter.CompId.Value : DBNull.Value),
                new SqlParameter("@BranchId", filter.BranchId.HasValue ? filter.BranchId.Value : DBNull.Value),
                new SqlParameter("@FromDate", filter.FromDate.HasValue ? filter.FromDate.Value.Date : DBNull.Value),
                new SqlParameter("@ToDate", filter.ToDate.HasValue ? filter.ToDate.Value.Date : DBNull.Value),
                new SqlParameter("@CustomerId", filter.CustomerId.HasValue ? filter.CustomerId.Value : DBNull.Value),
                new SqlParameter("@SearchText", string.IsNullOrWhiteSpace(filter.SearchText) ? (object)DBNull.Value : filter.SearchText.Trim()),
                new SqlParameter("@OnlyOutstanding", filter.OnlyOutstanding),
                new SqlParameter("@PageNumber", pageNumber),
                new SqlParameter("@PageSize", pageSize)
            };

            var resultList = await _dbHelper.ExecuteStoredProcedureAsync(
                "dbo.SP_CustomerWise_SalesReport",
                parameters,
                async reader =>
                {
                    var list = new List<CustomerWiseSalesReportModel>();
                    bool? hasCheckedError = null;
                    bool hasCheckedColumns = false;
                    bool hasSalesMasterInvoiceDate = false;
                    bool hasFirstSaleDate = false;
                    bool hasLastSaleDate = false;

                    while (await reader.ReadAsync(cancellationToken))
                    {
                        if (hasCheckedError != true)
                        {
                            if (HasColumn(reader, "Success") && HasColumn(reader, "Message") && !HasColumn(reader, "SrNo"))
                            {
                                var successVal = reader["Success"];
                                var isSuccess = successVal != DBNull.Value && (successVal is bool b ? b : Convert.ToInt32(successVal) == 1);
                                if (!isSuccess)
                                {
                                    var spMessage = reader["Message"] != DBNull.Value ? reader["Message"].ToString() : "Error executing stored procedure.";
                                    throw new InvalidOperationException(spMessage);
                                }
                            }
                            hasCheckedError = true;
                        }

                        if (!hasCheckedColumns)
                        {
                            hasSalesMasterInvoiceDate = HasColumn(reader, "SalesMaster_InvoiceDate");
                            hasFirstSaleDate = HasColumn(reader, "FirstSaleDate");
                            hasLastSaleDate = HasColumn(reader, "LastSaleDate");
                            hasCheckedColumns = true;
                        }

                        DateTime? invoiceDate = null;
                        if (hasSalesMasterInvoiceDate && reader["SalesMaster_InvoiceDate"] != DBNull.Value)
                        {
                            invoiceDate = Convert.ToDateTime(reader["SalesMaster_InvoiceDate"]);
                        }
                        else if (hasFirstSaleDate && reader["FirstSaleDate"] != DBNull.Value)
                        {
                            invoiceDate = Convert.ToDateTime(reader["FirstSaleDate"]);
                        }

                        DateTime? lastSaleDate = null;
                        if (hasLastSaleDate && reader["LastSaleDate"] != DBNull.Value)
                        {
                            lastSaleDate = Convert.ToDateTime(reader["LastSaleDate"]);
                        }

                        var item = new CustomerWiseSalesReportModel
                        {
                            SrNo = reader["SrNo"] != DBNull.Value ? Convert.ToInt64(reader["SrNo"]) : 0,
                            CompId = reader["CompId"] != DBNull.Value ? Convert.ToInt32(reader["CompId"]) : null,
                            BranchId = reader["BranchId"] != DBNull.Value ? Convert.ToInt32(reader["BranchId"]) : null,
                            CustomerId = reader["CustomerId"] != DBNull.Value ? Convert.ToInt32(reader["CustomerId"]) : null,
                            LedgerId = reader["LedgerId"] != DBNull.Value ? Convert.ToInt32(reader["LedgerId"]) : null,
                            CustomerName = reader["CustomerName"] != DBNull.Value ? reader["CustomerName"].ToString() : null,
                            MobileNo = reader["MobileNo"] != DBNull.Value ? reader["MobileNo"].ToString() : null,
                            BillingAddress = reader["BillingAddress"] != DBNull.Value ? reader["BillingAddress"].ToString() : null,
                            GSTNo = reader["GSTNo"] != DBNull.Value ? reader["GSTNo"].ToString() : null,
                            TotalInvoices = reader["TotalInvoices"] != DBNull.Value ? Convert.ToInt64(reader["TotalInvoices"]) : 0,
                            TotalQty = reader["TotalQty"] != DBNull.Value ? Convert.ToDecimal(reader["TotalQty"]) : 0,
                            SubTotal = reader["SubTotal"] != DBNull.Value ? Convert.ToDecimal(reader["SubTotal"]) : 0,
                            ItemDiscount = reader["ItemDiscount"] != DBNull.Value ? Convert.ToDecimal(reader["ItemDiscount"]) : 0,
                            BillWiseDiscount = reader["BillWiseDiscount"] != DBNull.Value ? Convert.ToDecimal(reader["BillWiseDiscount"]) : 0,
                            TotalCGST = reader["TotalCGST"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCGST"]) : 0,
                            TotalSGST = reader["TotalSGST"] != DBNull.Value ? Convert.ToDecimal(reader["TotalSGST"]) : 0,
                            TotalIGST = reader["TotalIGST"] != DBNull.Value ? Convert.ToDecimal(reader["TotalIGST"]) : 0,
                            TotalCESS = reader["TotalCESS"] != DBNull.Value ? Convert.ToDecimal(reader["TotalCESS"]) : 0,
                            TotalRoundOff = reader["TotalRoundOff"] != DBNull.Value ? Convert.ToDecimal(reader["TotalRoundOff"]) : 0,
                            TotalSales = reader["TotalSales"] != DBNull.Value ? Convert.ToDecimal(reader["TotalSales"]) : 0,
                            TotalReceived = reader["TotalReceived"] != DBNull.Value ? Convert.ToDecimal(reader["TotalReceived"]) : 0,
                            TotalOutstanding = reader["TotalOutstanding"] != DBNull.Value ? Convert.ToDecimal(reader["TotalOutstanding"]) : 0,
                            SalesMaster_InvoiceDate = invoiceDate,
                            FirstSaleDate = invoiceDate,
                            LastSaleDate = lastSaleDate
                        };

                        list.Add(item);
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            var totalRecords = (pageNumber == 1 && resultList.Count < pageSize) ? resultList.Count : 0;
            var totalPages = pageSize > 0 && totalRecords > 0 
                ? (int)Math.Ceiling(totalRecords / (double)pageSize) 
                : 0;

            var pagedResult = new PagedListResult<CustomerWiseSalesReportModel>
            {
                Items = resultList,
                TotalRecords = totalRecords,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return ApiResponse<PagedListResult<CustomerWiseSalesReportModel>>.SuccessResult(pagedResult, "Customer-wise sales report fetched successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetCustomerWiseSalesReportAsync");
            return ApiResponse<PagedListResult<CustomerWiseSalesReportModel>>.FailureResult(
                message: "Error fetching customer-wise sales report", 
                error: ex.Message,
                data: new PagedListResult<CustomerWiseSalesReportModel>());
        }
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
