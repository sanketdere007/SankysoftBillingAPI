using System;
using System.Collections.Generic;

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

public class DashboardChartsRequest
{
    public int? CompId { get; set; } = 1;
    public int? BranchId { get; set; } = 1;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? TopProducts { get; set; } = 5;
}

public class DailySalesTrendItem
{
    public DateTime SalesDate { get; set; }
    public string DayName { get; set; } = string.Empty;
    public long TotalBills { get; set; }
    public decimal SalesAmount { get; set; }
}

public class PaymentCollectionItem
{
    public string CollectionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class MonthlySalesVsPurchaseItem
{
    public DateTime MonthStart { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal SalesAmount { get; set; }
    public decimal PurchaseAmount { get; set; }
}

public class TopSellingProductItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalQty { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalBills { get; set; }
}

public class DashboardChartsResponse
{
    public List<DailySalesTrendItem> DailySalesTrend { get; set; } = new();
    public List<PaymentCollectionItem> PaymentCollection { get; set; } = new();
    public List<MonthlySalesVsPurchaseItem> MonthlySalesVsPurchase { get; set; } = new();
    public List<TopSellingProductItem> TopSellingProducts { get; set; } = new();
}
