using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class BrandModel
{
    public int Brand_Id { get; set; } = 0;

    [Required(ErrorMessage = "Brand Name is required.")]
    [StringLength(100, ErrorMessage = "Brand Name cannot exceed 100 characters.")]
    public string Brand_Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Brand Description cannot exceed 250 characters.")]
    public string? Brand_Description { get; set; }

    public bool Brand_IsActive { get; set; } = true;
    public int Brand_CreatedBy { get; set; } = 0;
    public int Brand_ModifiedBy { get; set; } = 0;
}

public class BrandSaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Brand_Id { get; set; }
}

public class BrandListModel : BrandModel
{
    public DateTime? Brand_CreatedDate { get; set; }
    public DateTime? Brand_ModifiedDate { get; set; }
}

public class BrandFilterDto
{
    public int? Brand_Id { get; set; }
    public bool? Brand_IsActive { get; set; }
}
