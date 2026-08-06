using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

/// <summary>
/// Area model representing area data for insert and update operations.
/// Mapped to SP_Area_InsertOrUpdate stored procedure JSON payload.
/// </summary>
public class AreaModel
{
    /// <summary>
    /// Area ID (0 for Insert, > 0 for Update)
    /// </summary>
    public int Area_Id { get; set; } = 0;

    /// <summary>
    /// State Identifier foreign key
    /// </summary>
    [Required(ErrorMessage = "State ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Area_StateId must be a valid positive integer.")]
    public int Area_StateId { get; set; }

    /// <summary>
    /// City Identifier foreign key
    /// </summary>
    [Required(ErrorMessage = "City ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Area_CityId must be a valid positive integer.")]
    public int Area_CityId { get; set; }

    /// <summary>
    /// Area / Locality Name
    /// </summary>
    [Required(ErrorMessage = "Area Name is required.")]
    [StringLength(150, ErrorMessage = "Area Name cannot exceed 150 characters.")]
    public string Area_Name { get; set; } = string.Empty;

    /// <summary>
    /// Area Postal Pincode
    /// </summary>
    [Required(ErrorMessage = "Pincode is required.")]
    [StringLength(10, ErrorMessage = "Area Pincode cannot exceed 10 characters.")]
    public string Area_Pincode { get; set; } = string.Empty;

    /// <summary>
    /// Active Status flag
    /// </summary>
    public bool Area_IsActive { get; set; } = true;

    /// <summary>
    /// User ID of creator
    /// </summary>
    public int Area_CreatedBy { get; set; } = 0;

    /// <summary>
    /// User ID of modifier
    /// </summary>
    public int Area_ModifiedBy { get; set; } = 0;
}

/// <summary>
/// Result returned by the SP_Area_InsertOrUpdate stored procedure.
/// </summary>
public class AreaSaveResult
{
    /// <summary>
    /// Status indicating whether the insert or update succeeded (true/1) or failed (false/0).
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Informational message returned by the stored procedure (e.g., 'Area Added Successfully.', 'Area Updated Successfully.', 'Area Already Exists.').
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The generated or updated Area ID (if returned).
    /// </summary>
    public int? Area_Id { get; set; }
}

/// <summary>
/// Model representing area data returned from SP_Area_GetAll stored procedure.
/// </summary>
public class AreaListModel
{
    /// <summary>
    /// Area Identifier
    /// </summary>
    public int Area_Id { get; set; }

    /// <summary>
    /// Area / Locality Name
    /// </summary>
    public string Area_Name { get; set; } = string.Empty;

    /// <summary>
    /// Postal Pincode
    /// </summary>
    public string Area_Pincode { get; set; } = string.Empty;

    /// <summary>
    /// City Name
    /// </summary>
    public string City_Name { get; set; } = string.Empty;

    /// <summary>
    /// State Name
    /// </summary>
    public string State_Name { get; set; } = string.Empty;

    /// <summary>
    /// Active Status
    /// </summary>
    public bool Area_IsActive { get; set; }
}

/// <summary>
/// Query filter parameters for SP_Area_GetAll stored procedure.
/// </summary>
public class AreaFilterDto
{
    /// <summary>
    /// Search term matching Area Name, City Name, State Name, or Pincode
    /// </summary>
    public string? Search { get; set; } = string.Empty;

    /// <summary>
    /// Filter by State ID
    /// </summary>
    public int? StateId { get; set; }

    /// <summary>
    /// Filter by City ID
    /// </summary>
    public int? CityId { get; set; }

    /// <summary>
    /// Filter by Pincode
    /// </summary>
    public string? Pincode { get; set; } = string.Empty;

    /// <summary>
    /// Filter by Active Status (1 = Active, 0 = Inactive, null = All)
    /// </summary>
    public bool? IsActive { get; set; }
}
