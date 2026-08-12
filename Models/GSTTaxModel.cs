using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class GSTTaxModel
{
    public int GSTTax_Id { get; set; } = 0;

    [Required(ErrorMessage = "GST Tax Name is required.")]
    [StringLength(50, ErrorMessage = "GST Tax Name cannot exceed 50 characters.")]
    public string GSTTax_Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "GST Percentage is required.")]
    [Range(0, 100, ErrorMessage = "GST Percentage must be between 0 and 100.")]
    public decimal GSTTax_Percentage { get; set; } = 0m;

    public decimal GSTTax_CGST { get; set; } = 0m;
    public decimal GSTTax_SGST { get; set; } = 0m;
    public decimal GSTTax_IGST { get; set; } = 0m;

    public bool GSTTax_IsActive { get; set; } = true;
    public int GSTTax_CreatedBy { get; set; } = 0;
    public int GSTTax_ModifiedBy { get; set; } = 0;
}

public class GSTTaxSaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int GSTTax_Id { get; set; }
}

public class GSTTaxListModel : GSTTaxModel
{
    public DateTime? GSTTax_CreatedDate { get; set; }
    public DateTime? GSTTax_ModifiedDate { get; set; }
}

public class GSTTaxFilterDto
{
    public int? GSTTax_Id { get; set; }
    public bool? GSTTax_IsActive { get; set; }
}
