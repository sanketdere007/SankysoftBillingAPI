namespace Billing_Software_Api.Models;

/// <summary>
/// Invoice-level pending amount row returned from SP_SalesEntry_PendingAmount_GetAll.
/// </summary>
public class SalesPendingAmountModel
{
    public int SalesMaster_Id { get; set; }
    public string? SalesMaster_InvoiceNo { get; set; }
    public DateTime? SalesMaster_InvoiceDate { get; set; }

    public int? SalesMaster_LedgerId { get; set; }
    public string? AccLedger_Name { get; set; }

    public decimal SalesMaster_GrandTotal { get; set; }
    public decimal SalesMaster_PaidAmount { get; set; }
    public decimal SalesMaster_BalanceAmount { get; set; }

    public string? Cust_Code { get; set; }
    public string? Cust_Name { get; set; }
    public string? Cust_MobileNo { get; set; }
}

/// <summary>
/// Query filter parameters for SP_SalesEntry_PendingAmount_GetAll.
/// </summary>
public class SalesPendingAmountFilterDto
{
    public int? CompId { get; set; }
    public int? BranchId { get; set; }
    public int? CustomerId { get; set; }
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
