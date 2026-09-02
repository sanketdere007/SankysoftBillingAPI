using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class BatchListModel
{
    public int Batch_Id { get; set; }
    public int? Batch_ProductId { get; set; }
    public string? Prod_Name { get; set; }
    public string? Prod_Code { get; set; }
    public string? Unit_Name { get; set; }
    public decimal? Prod_UnitValue { get; set; }
    public int? Batch_CompId { get; set; }
    public string? Comp_Name { get; set; }
    public int? Batch_BranchId { get; set; }
    public string? Branch_Name { get; set; }
    public decimal? Batch_Stock { get; set; }
    public decimal? Batch_AvailableStock { get; set; }
    public decimal? Batch_LandingPrice { get; set; }
    public decimal? Batch_PurchasePrice { get; set; }
    public decimal? Batch_MRP { get; set; }
    public decimal? Batch_SellingPrice { get; set; }
}

public class BatchFilterDto
{
    public int? CompId { get; set; }
    public int? BranchId { get; set; }
    public int? ProductId { get; set; }
    public string? Search { get; set; }
}
