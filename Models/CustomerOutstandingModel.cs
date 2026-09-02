namespace Billing_Software_Api.Models;

/// <summary>
/// Customer-level outstanding row returned from SP_Customer_Outstanding_GetAll.
/// </summary>
public class CustomerOutstandingModel
{
    public int CustomerId { get; set; }
    public string? Cust_Code { get; set; }
    public string? Cust_Name { get; set; }
    public string? Cust_MobileNo { get; set; }

    public decimal TotalInvoiceAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalOutstanding { get; set; }
}

/// <summary>
/// Query filter parameters for SP_Customer_Outstanding_GetAll.
/// </summary>
public class CustomerOutstandingFilterDto
{
    public int? CompId { get; set; }
    public int? BranchId { get; set; }
    public int? CustomerId { get; set; }
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
