namespace Billing_Software_Api.Models;

/// <summary>
/// Query filter DTO for SP_StockLedger_Report.
/// </summary>
public class StockLedgerReportFilterDto
{
    public int? CompId { get; set; } = 1;
    public int? BranchId { get; set; } = 1;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? ProductId { get; set; }
    public int? BatchId { get; set; }
    public string? TransactionType { get; set; }
    public string? Type { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

/// <summary>
/// Data model representing a single stock ledger entry returned by SP_StockLedger_Report.
/// </summary>
public class StockLedgerReportModel
{
    public long StockLedger_Id { get; set; }
    public int StockLedger_CompId { get; set; }
    public int StockLedger_BranchId { get; set; }
    public int StockLedger_ProductId { get; set; }
    public int? StockLedger_BatchId { get; set; }
    public DateTime? StockLedger_Date { get; set; }
    public string? StockLedger_Type { get; set; }
    public string? StockLedger_TransactionType { get; set; }
    public int? StockLedger_ReferenceId { get; set; }
    public string? StockLedger_ReferenceNo { get; set; }
    public decimal Quantity { get; set; }
    public decimal BeforeStock { get; set; }
    public decimal AfterStock { get; set; }
    public decimal? PhysicalStock { get; set; }
    public decimal? AdjustmentQty { get; set; }
    public string? StockLedger_Remark { get; set; }
    public int? StockLedger_CreatedBy { get; set; }
    public DateTime? StockLedger_CreatedDate { get; set; }
    public int TotalRecords { get; set; }

    // Optional display fields if joined by extended queries
    public string? Prod_Name { get; set; }
    public string? Batch_Number { get; set; }
}
