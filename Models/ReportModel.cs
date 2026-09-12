using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class ProductWiseSalesReportFilterDto
{
    public int CompId { get; set; } = 1;
    public int BranchId { get; set; } = 1;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchText { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortColumn { get; set; } = "TotalAmount";
    public string SortDirection { get; set; } = "DESC";
}

public class ProductWiseSalesReportModel
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? HSNCode { get; set; }
    public decimal TotalQty { get; set; }
    public decimal TotalFreeQty { get; set; }
    public decimal TotalOverallQty { get; set; }
    public decimal TotalTaxableAmount { get; set; }
    public decimal TotalTaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalInvoices { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class ProductWiseCustomerPurchaseListFilterDto
{
    public int ProductId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CompId { get; set; }
    public int? BranchId { get; set; }
    public string? SearchText { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class ProductWiseCustomerPurchaseListModel
{
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerMobile { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public int SalesMasterId { get; set; }
    public decimal Qty { get; set; }
    public decimal FreeQty { get; set; }
    public decimal TotalQty { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
