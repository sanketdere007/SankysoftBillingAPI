namespace Billing_Software_Api.Models;

/// <summary>
/// Query filter DTO for SP_ProductWise_PurchaseReport.
/// </summary>
public class ProductWisePurchaseReportFilterDto
{
    public int? CompId { get; set; } = 1;
    public int? BranchId { get; set; } = 1;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? ProductId { get; set; }
    public int? SupplierId { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Data model representing a product's aggregated purchase summary record returned by SP_ProductWise_PurchaseReport.
/// </summary>
public class ProductWisePurchaseReportModel
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal TotalPurchaseQty { get; set; }
    public int TotalInvoices { get; set; }
    public int TotalPurchaseLines { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalGST { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
    public DateTime? FirstPurchaseDate { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public int TotalRecords { get; set; }
}
