using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class CategoryModel
{
    public int Cat_Id { get; set; } = 0;

    [Required(ErrorMessage = "Category Name is required.")]
    [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters.")]
    public string Cat_Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Category Description cannot exceed 250 characters.")]
    public string? Cat_Description { get; set; }

    public bool Cat_IsActive { get; set; } = true;
    public int Cat_CreatedBy { get; set; } = 0;
    public int Cat_ModifiedBy { get; set; } = 0;
}

public class CategorySaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Cat_Id { get; set; }
}

public class CategoryListModel : CategoryModel
{
    public DateTime? Cat_CreatedDate { get; set; }
    public DateTime? Cat_ModifiedDate { get; set; }
}

public class CategoryFilterDto
{
    public int? Cat_Id { get; set; }
    public bool? Cat_IsActive { get; set; }
}
