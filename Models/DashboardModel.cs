using System;

namespace Billing_Software_Api.Models;

public class DashboardSummaryRequest
{
    public int? CompId { get; set; }
    public int? BranchId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? LowStockQty { get; set; }
}

public class DashboardSummaryResponse
{
    public int LowStockProducts { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalPurchase { get; set; }
    public int OutOfStockProducts { get; set; }
    public int TotalProducts { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal Collection { get; set; }
    public int TotalSalesOrders { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
