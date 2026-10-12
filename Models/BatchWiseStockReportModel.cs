namespace Billing_Software_Api.Models;

/// <summary>
/// Filter DTO for SP_BatchWiseStockReport.
/// </summary>
public class BatchWiseStockReportFilterDto
{
    public int? CompId { get; set; } = 1;
    public int? BranchId { get; set; } = 1;
    public int? ProductId { get; set; }
    public string? Search { get; set; }
    public bool OnlyInStock { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 500;
}

/// <summary>
/// Data model representing a single record returned by SP_BatchWiseStockReport.
/// </summary>
public class BatchWiseStockReportModel
{
    public int Batch_Id { get; set; }
    public int? Batch_CompId { get; set; }
    public int? Batch_BranchId { get; set; }
    public int? Batch_ProductId { get; set; }
    public string? Prod_Name { get; set; }
    public decimal? Prod_UnitValue { get; set; }
    public string? Unit_ShortName { get; set; }
    public string? Batch_Barcode { get; set; }
    public string? Batch_EANCode { get; set; }
    public string? Batch_Number { get; set; }
    public decimal? Batch_Stock { get; set; }
    public decimal? Batch_AvailableStock { get; set; }
    public decimal? Batch_LandingPrice { get; set; }
    public decimal? Batch_PurchasePrice { get; set; }
    public decimal? Batch_MRP { get; set; }
    public decimal? Batch_SellingPrice { get; set; }
    public int TotalRecords { get; set; }
}
