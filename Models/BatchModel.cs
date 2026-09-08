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

public class ProductStockFilterDto
{
    public int? CompId { get; set; }
    public int? BranchId { get; set; }
    public int? ProductId { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class ProductStockModel
{
    public int? Batch_CompId { get; set; }
    public int? Batch_BranchId { get; set; }
    public int? Batch_ProductId { get; set; }
    public string? ProductName { get; set; }

    public int? Prod_UnitId { get; set; }
    public decimal? Prod_UnitValue { get; set; }
    public string? UnitName { get; set; }
    public string? UnitShortName { get; set; }

    public int? Prod_BrandId { get; set; }
    public string? BrandName { get; set; }

    public int? Prod_CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public int? Prod_SubCategoryId { get; set; }
    public string? SubCategoryName { get; set; }

    public decimal? Batch_Stock { get; set; }
    public decimal? Batch_AvailableStock { get; set; }

    public decimal? Batch_LandingPrice { get; set; }
    public decimal? Batch_PurchasePrice { get; set; }
    public decimal? Batch_MRP { get; set; }
    public decimal? Batch_SellingPrice { get; set; }

    public int? Batch_IsActive { get; set; }
    public DateTime? Batch_CreatedDate { get; set; }
    public DateTime? Batch_ModifiedDate { get; set; }
}

public class BatchSaveModel
{
    public int Batch_Id { get; set; }
    [Required]
    public int Batch_CompId { get; set; }
    [Required]
    public int Batch_BranchId { get; set; }
    [Required]
    public int Batch_ProductId { get; set; }
    public string? Batch_Barcode { get; set; }
    public string? Batch_EANCode { get; set; }
    public decimal Batch_Stock { get; set; }
    public decimal Batch_AvailableStock { get; set; }
    public decimal Batch_LandingPrice { get; set; }
    public decimal Batch_PurchasePrice { get; set; }
    public decimal Batch_MRP { get; set; }
    public decimal Batch_SellingPrice { get; set; }
    public bool Batch_IsActive { get; set; } = true;
    public int? Batch_CreatedBy { get; set; }
    public int? Batch_ModifiedBy { get; set; }
}

public class BatchSaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Batch_Id { get; set; }
    public decimal? OldStock { get; set; }
    public decimal? OldAvailableStock { get; set; }
    public decimal? NewStock { get; set; }
    public decimal? NewAvailableStock { get; set; }
}
