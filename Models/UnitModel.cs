using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class UnitModel
{
    public int Unit_Id { get; set; } = 0;

    [Required(ErrorMessage = "Unit Name is required.")]
    [StringLength(100, ErrorMessage = "Unit Name cannot exceed 100 characters.")]
    public string Unit_Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Unit Short Name is required.")]
    [StringLength(20, ErrorMessage = "Unit Short Name cannot exceed 20 characters.")]
    public string Unit_ShortName { get; set; } = string.Empty;

    public bool Unit_IsActive { get; set; } = true;
    public int Unit_CreatedBy { get; set; } = 0;
    public int Unit_ModifiedBy { get; set; } = 0;
}

public class UnitSaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Unit_Id { get; set; }
}

public class UnitListModel : UnitModel
{
    public DateTime? Unit_CreatedDate { get; set; }
    public DateTime? Unit_ModifiedDate { get; set; }
}

public class UnitFilterDto
{
    public int? Unit_Id { get; set; }
    public bool? Unit_IsActive { get; set; }
}
