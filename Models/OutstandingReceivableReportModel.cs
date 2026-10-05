namespace Billing_Software_Api.Models;

public class OutstandingReceivableReportFilterDto
{
    public int CompId { get; set; } = 1;
    public int BranchId { get; set; } = 1;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public int? CustomerId { get; set; }
    public int? RouteId { get; set; }
    public int? AreaId { get; set; }
    public int? CityId { get; set; }
    public int? StateId { get; set; }
    public bool OnlyOutstanding { get; set; } = true;
}

public class OutstandingReceivableReportModel
{
    public List<OutstandingInvoiceDetailModel> InvoiceDetails { get; set; } = new();
    public List<OutstandingPartyTotalModel> PartyTotals { get; set; } = new();
    public OutstandingGrandTotalModel? GrandTotal { get; set; }
}

public class OutstandingInvoiceDetailModel
{
    public int Cust_Id { get; set; }
    public string? Cust_Code { get; set; }
    public string? Cust_Name { get; set; }
    public string? Cust_MobileNo { get; set; }
    
    public int? Cust_StateId { get; set; }
    public string? State_Name { get; set; }
    public int? Cust_CityId { get; set; }
    public string? City_Name { get; set; }
    public int? Cust_AreaId { get; set; }
    public string? Area_Name { get; set; }
    public int? Cust_RouteId { get; set; }
    public string? Route_Name { get; set; }
    
    public int SalesMaster_Id { get; set; }
    public string? SalesMaster_InvoiceNo { get; set; }
    public DateTime? SalesMaster_InvoiceDate { get; set; }
    
    public decimal BillAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public int DaysOutstanding { get; set; }
    public string? RowType { get; set; }
}

public class OutstandingPartyTotalModel
{
    public int Cust_Id { get; set; }
    public string? Cust_Code { get; set; }
    public string? Cust_Name { get; set; }
    
    public int TotalInvoices { get; set; }
    public decimal TotalBillAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalBalanceAmount { get; set; }
    public string? RowType { get; set; }
}

public class OutstandingGrandTotalModel
{
    public int TotalInvoices { get; set; }
    public int TotalCustomers { get; set; }
    public decimal GrandTotalBillAmount { get; set; }
    public decimal GrandTotalPaidAmount { get; set; }
    public decimal GrandTotalBalanceAmount { get; set; }
}
