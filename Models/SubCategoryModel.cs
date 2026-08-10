using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class SubCategoryModel
{
    public int SubCat_Id { get; set; } = 0;

    [Required(ErrorMessage = "Category ID is required.")]
    public int SubCat_CatId { get; set; }

    [Required(ErrorMessage = "Sub-category Name is required.")]
    [StringLength(100, ErrorMessage = "Sub-category Name cannot exceed 100 characters.")]
    public string SubCat_Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Sub-category Description cannot exceed 250 characters.")]
    public string? SubCat_Description { get; set; }

    public bool SubCat_IsActive { get; set; } = true;
    public int SubCat_CreatedBy { get; set; } = 0;
    public int SubCat_ModifiedBy { get; set; } = 0;
}

public class SubCategorySaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int SubCat_Id { get; set; }
}

public class SubCategoryListModel : SubCategoryModel
{
    public string? Cat_Name { get; set; }
    public DateTime? SubCat_CreatedDate { get; set; }
    public DateTime? SubCat_ModifiedDate { get; set; }
}

public class SubCategoryFilterDto
{
    public int? SubCat_Id { get; set; }
    public bool? SubCat_IsActive { get; set; }
}
