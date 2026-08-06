using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

/// <summary>
/// City model representing city data for insert and update operations.
/// Mapped to SP_City_InsertOrUpdate stored procedure JSON payload.
/// </summary>
public class CityModel
{
    /// <summary>
    /// City ID (0 for Insert, > 0 for Update)
    /// </summary>
    public int City_Id { get; set; } = 0;

    /// <summary>
    /// State Identifier foreign key
    /// </summary>
    [Required(ErrorMessage = "State ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "City_StateId must be a valid positive integer.")]
    public int City_StateId { get; set; }

    /// <summary>
    /// City Name
    /// </summary>
    [Required(ErrorMessage = "City Name is required.")]
    [StringLength(100, ErrorMessage = "City Name cannot exceed 100 characters.")]
    public string City_Name { get; set; } = string.Empty;

    /// <summary>
    /// Active Status flag
    /// </summary>
    public bool City_IsActive { get; set; } = true;

    /// <summary>
    /// User ID of creator
    /// </summary>
    public int City_CreatedBy { get; set; } = 0;

    /// <summary>
    /// User ID of modifier
    /// </summary>
    public int City_ModifiedBy { get; set; } = 0;
}

/// <summary>
/// Result returned by the SP_City_InsertOrUpdate stored procedure.
/// </summary>
public class CitySaveResult
{
    /// <summary>
    /// Status indicating whether the insert or update succeeded (true/1) or failed (false/0).
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Informational message returned by the stored procedure (e.g., 'City Added Successfully.', 'City Updated Successfully.', 'City Already Exists.').
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The generated or updated City ID (if returned).
    /// </summary>
    public int? City_Id { get; set; }
}

/// <summary>
/// Model representing city data returned from SP_City_GetAll stored procedure.
/// </summary>
public class CityListModel
{
    /// <summary>
    /// City Identifier
    /// </summary>
    public int City_Id { get; set; }

    /// <summary>
    /// City Name
    /// </summary>
    public string City_Name { get; set; } = string.Empty;

    /// <summary>
    /// State Name
    /// </summary>
    public string State_Name { get; set; } = string.Empty;

    /// <summary>
    /// State Code (e.g., MH, KA, GJ)
    /// </summary>
    public string State_Code { get; set; } = string.Empty;

    /// <summary>
    /// Active Status
    /// </summary>
    public bool City_IsActive { get; set; }
}

/// <summary>
/// Query filter parameters for SP_City_GetAll stored procedure.
/// </summary>
public class CityFilterDto
{
    /// <summary>
    /// Search term matching City Name, State Name, or State Code
    /// </summary>
    public string? Search { get; set; } = string.Empty;

    /// <summary>
    /// Filter by State ID
    /// </summary>
    public int? StateId { get; set; }

    /// <summary>
    /// Filter by Active Status (1 = Active, 0 = Inactive, null = All)
    /// </summary>
    public bool? IsActive { get; set; }
}
