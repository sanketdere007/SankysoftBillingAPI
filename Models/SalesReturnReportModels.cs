using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

#region 1. Product-Wise Sales Return Report

/// <summary>
/// Filter DTO for dbo.SP_ProductWise_SalesReturnReport.
/// </summary>
public class ProductWiseSalesReturnReportFilterDto
{
    public int? CompId { get; set; } = 1;
    public int? BranchId { get; set; } = 1;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? ProductId { get; set; }
    public string? Search { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Data model representing a product's aggregated sales return summary record returned by dbo.SP_ProductWise_SalesReturnReport.
/// </summary>
public class ProductWiseSalesReturnReportModel
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }

    public int TotalReturnEntries { get; set; }
    public int TotalBatches { get; set; }

    public DateTime? FirstReturnDate { get; set; }
    public DateTime? LastReturnDate { get; set; }

    public decimal TotalReturnQty { get; set; }
    public decimal TotalFreeQty { get; set; }
    public decimal TotalReturnedUnits { get; set; }

    public decimal TotalSubTotal { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalReturnAmount { get; set; }

    public decimal ReturnQtyValue { get; set; }
    public decimal ReturnMRPValue { get; set; }

    public long TotalRecords { get; set; }
    public long RowNumber { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

#endregion

#region 2. Product-Wise Sales Return Detail Report

/// <summary>
/// Filter DTO for dbo.SP_ProductWise_SalesReturnDetailReport.
/// </summary>
public class ProductWiseSalesReturnDetailReportFilterDto
{
    public int? CompId { get; set; } = 1;
    public int? BranchId { get; set; } = 1;

    [Required(ErrorMessage = "Valid ProductId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0.")]
    public int ProductId { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CustomerId { get; set; }
    public int? BatchId { get; set; }
    public string? InvoiceNo { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Line-item detail returned by dbo.SP_ProductWise_SalesReturnDetailReport.
/// </summary>
public class ProductWiseSalesReturnDetailItemModel
{
    public int SalesReturnMaster_Id { get; set; }
    public string? SalesReturnMaster_InvoiceNo { get; set; }
    public DateTime? SalesReturnMaster_Date { get; set; }
    public int SalesReturnMaster_CompId { get; set; }
    public int SalesReturnMaster_BranchId { get; set; }
    public int? SalesReturnMaster_CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? SalesReturnMaster_LedgerId { get; set; }
    public string? SalesReturnMaster_Status { get; set; }
    public string? SalesReturnMaster_Remark { get; set; }

    public int SalesReturnDetail_ProductId { get; set; }
    public int? SalesReturnDetail_BatchId { get; set; }
    public int? SalesReturnDetail_UnitId { get; set; }
    public decimal SalesReturnDetail_Qty { get; set; }
    public decimal SalesReturnDetail_FreeQty { get; set; }
    public decimal SalesReturnDetail_SellingPrice { get; set; }
    public decimal SalesReturnDetail_MRP { get; set; }
    public decimal SalesReturnDetail_DiscountPercentage { get; set; }
    public decimal SalesReturnDetail_DiscountAmount { get; set; }
    public decimal SalesReturnDetail_TaxPercentage { get; set; }
    public decimal SalesReturnDetail_TaxAmount { get; set; }
    public decimal SalesReturnDetail_SubTotal { get; set; }
    public decimal SalesReturnDetail_TotalAmount { get; set; }
    public string? SalesReturnDetail_Remark { get; set; }

    public long RowNumber { get; set; }
}

/// <summary>
/// Paginated container with overall window aggregates for dbo.SP_ProductWise_SalesReturnDetailReport.
/// </summary>
public class ProductWiseSalesReturnDetailReportModel
{
    public List<ProductWiseSalesReturnDetailItemModel> Items { get; set; } = new();
    public long TotalRecords { get; set; }
    public decimal TotalReturnQty { get; set; }
    public decimal TotalFreeQty { get; set; }
    public decimal TotalReturnAmount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

#endregion

#region 3. Customer-Wise Sales Return Report

/// <summary>
/// Filter DTO for dbo.SP_CustomerWise_SalesReturnReport.
/// </summary>
public class CustomerWiseSalesReturnReportFilterDto
{
    public int? CompId { get; set; } = 1;
    public int? BranchId { get; set; } = 1;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CustomerId { get; set; }
    public string? Search { get; set; }
    public bool OnlyOutstanding { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Result 1: Paginated return statement row returned by dbo.SP_CustomerWise_SalesReturnReport.
/// </summary>
public class CustomerWiseSalesReturnStatementModel
{
    public int? CustomerId { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }
    public string? MobileNo { get; set; }

    public int ReturnId { get; set; }
    public string? BillNo { get; set; }
    public DateTime? ReturnDate { get; set; }

    public decimal BillAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }

    public int? Days { get; set; }
    public int? SalesmanId { get; set; }

    public string? RowType { get; set; }
}

/// <summary>
/// Result 2: Customer / Party summary total returned by dbo.SP_CustomerWise_SalesReturnReport.
/// </summary>
public class CustomerWiseSalesReturnPartyTotalModel
{
    public int? CustomerId { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }

    public int TotalReturns { get; set; }
    public decimal TotalBillAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalBalanceAmount { get; set; }

    public string? RowType { get; set; }
}

/// <summary>
/// Result 3: Grand summary total returned by dbo.SP_CustomerWise_SalesReturnReport.
/// </summary>
public class CustomerWiseSalesReturnGrandTotalModel
{
    public int TotalReturns { get; set; }
    public int TotalCustomers { get; set; }
    public decimal GrandTotalBillAmount { get; set; }
    public decimal GrandTotalPaidAmount { get; set; }
    public decimal GrandTotalBalanceAmount { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// Comprehensive response model containing the 3 result sets of dbo.SP_CustomerWise_SalesReturnReport.
/// </summary>
public class CustomerWiseSalesReturnReportModel
{
    public List<CustomerWiseSalesReturnStatementModel> Statement { get; set; } = new();
    public List<CustomerWiseSalesReturnPartyTotalModel> PartyTotals { get; set; } = new();
    public CustomerWiseSalesReturnGrandTotalModel? GrandTotal { get; set; }
}

#endregion
