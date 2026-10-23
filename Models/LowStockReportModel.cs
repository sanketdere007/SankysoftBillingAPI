namespace Billing_Software_Api.Models;

/// <summary>
/// Query filter DTO for SP_LowStock_Report.
/// </summary>
public class LowStockReportFilterDto
{
    public int? CompId { get; set; } = 1;
    public int? BranchId { get; set; } = 1;
    public decimal? LowStockQty { get; set; } = 10;
    public int? ProductId { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Data model representing a single record returned by SP_LowStock_Report.
/// </summary>
public class LowStockReportModel
{
    public int? Batch_CompId { get; set; }
    public int? Batch_BranchId { get; set; }
    public int Batch_ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? UnitShortName { get; set; }
    public decimal? UnitValue { get; set; }
    public decimal TotalStock { get; set; }
    public decimal AvailableStock { get; set; }
    public decimal StockShortage { get; set; }
    public int BatchCount { get; set; }
    public int TotalRecords { get; set; }
}
