using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class ProductModel
{
    public int Prod_Id { get; set; } = 0;
    
    [Required(ErrorMessage = "Company ID is required.")]
    public int Prod_CompId { get; set; }
    
    [Required(ErrorMessage = "Branch ID is required.")]
    public int Prod_BranchId { get; set; }
    
    [StringLength(50, ErrorMessage = "Product Code cannot exceed 50 characters.")]
    public string? Prod_Code { get; set; }
    
    [Required(ErrorMessage = "Product Name is required.")]
    [StringLength(200, ErrorMessage = "Product Name cannot exceed 200 characters.")]
    public string Prod_Name { get; set; } = string.Empty;
    
    public int? Prod_BrandId { get; set; }
    public int? Prod_CategoryId { get; set; }
    public int? Prod_SubCategoryId { get; set; }
    public int? Prod_UnitId { get; set; }
    public decimal? Prod_UnitValue { get; set; }
    
    [StringLength(50, ErrorMessage = "HSN Code cannot exceed 50 characters.")]
    public string? Prod_HSNCode { get; set; }
    public decimal? Prod_GSTPercent { get; set; }
    
    [StringLength(50, ErrorMessage = "Barcode cannot exceed 50 characters.")]
    public string? Batch_Barcode { get; set; }
    [StringLength(50, ErrorMessage = "EAN Code cannot exceed 50 characters.")]
    public string? Batch_EANCode { get; set; }
    public decimal? Batch_Stock { get; set; }
    public decimal? Batch_LandingPrice { get; set; }
    public decimal? Batch_PurchasePrice { get; set; }
    public decimal? Batch_MRP { get; set; }
    public decimal? Batch_SellingPrice { get; set; }
    
    public bool Prod_IsActive { get; set; } = true;
    public int Prod_CreatedBy { get; set; } = 0;
    public int Prod_ModifiedBy { get; set; } = 0;
}

public class ProductSaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Prod_Id { get; set; }
}

public class ProductListModel : ProductModel
{
    public string? Prod_CompanyName { get; set; }
    public string? Prod_BranchName { get; set; }
    public string? Prod_BrandName { get; set; }
    public string? Prod_CategoryName { get; set; }
    public string? Prod_SubCategoryName { get; set; }
    public string? Prod_UnitName { get; set; }
    public string? Prod_UnitShortName { get; set; }
    
    public DateTime? Prod_CreatedDate { get; set; }
    public DateTime? Prod_ModifiedDate { get; set; }
}

public class ProductFilterDto
{
    public int? Prod_Id { get; set; }
    public bool? Prod_IsActive { get; set; }
}
