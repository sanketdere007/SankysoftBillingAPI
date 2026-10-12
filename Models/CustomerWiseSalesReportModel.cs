namespace Billing_Software_Api.Models;

/// <summary>
/// Query filter DTO for SP_CustomerWise_SalesReport.
/// </summary>
public class CustomerWiseSalesReportFilterDto
{
    public int? CompId { get; set; } = 1;
    public int? BranchId { get; set; } = 1;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CustomerId { get; set; }
    public string? SearchText { get; set; }
    public bool OnlyOutstanding { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Data model representing a customer's aggregated sales summary record returned by SP_CustomerWise_SalesReport.
/// </summary>
public class CustomerWiseSalesReportModel
{
    public long SrNo { get; set; }
    public int? CompId { get; set; }
    public int? BranchId { get; set; }
    public int? CustomerId { get; set; }
    public int? LedgerId { get; set; }
    public string? CustomerName { get; set; }
    public string? MobileNo { get; set; }
    public string? BillingAddress { get; set; }
    public string? GSTNo { get; set; }
    public long TotalInvoices { get; set; }
    public decimal TotalQty { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ItemDiscount { get; set; }
    public decimal BillWiseDiscount { get; set; }
    public decimal TotalCGST { get; set; }
    public decimal TotalSGST { get; set; }
    public decimal TotalIGST { get; set; }
    public decimal TotalCESS { get; set; }
    public decimal TotalRoundOff { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalOutstanding { get; set; }
    public DateTime? SalesMaster_InvoiceDate { get; set; }
    public DateTime? FirstSaleDate { get; set; }
    public DateTime? LastSaleDate { get; set; }
}
