using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

public class RouteModel
{
    public int Route_Id { get; set; } = 0;
    
    [Required(ErrorMessage = "Company Id is required.")]
    public int Route_CompId { get; set; }
    
    [Required(ErrorMessage = "Branch Id is required.")]
    public int Route_BranchId { get; set; }

    [Required(ErrorMessage = "Route Name is required.")]
    [StringLength(150, ErrorMessage = "Route Name cannot exceed 150 characters.")]
    public string Route_Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Route Description cannot exceed 500 characters.")]
    public string? Route_Description { get; set; }

    public bool Route_IsActive { get; set; } = true;
    public int Route_CreatedBy { get; set; } = 0;
    public int Route_ModifiedBy { get; set; } = 0;
}

public class RouteSaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Route_Id { get; set; }
}

public class RouteListModel : RouteModel
{
    public DateTime? Route_CreatedDate { get; set; }
    public DateTime? Route_ModifiedDate { get; set; }
}

public class RouteFilterDto
{
    public int? Route_CompId { get; set; }
    public int? Route_BranchId { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
