using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

/// <summary>
/// State model representing state master data.
/// </summary>
public class StateModel
{
    /// <summary>
    /// State ID (0 for Insert, > 0 for Update)
    /// </summary>
    public int State_Id { get; set; } = 0;

    /// <summary>
    /// State Name
    /// </summary>
    [Required(ErrorMessage = "State Name is required.")]
    [StringLength(100, ErrorMessage = "State Name cannot exceed 100 characters.")]
    public string State_Name { get; set; } = string.Empty;

    /// <summary>
    /// State Code (e.g., MH, GJ, KA, DL)
    /// </summary>
    [StringLength(10, ErrorMessage = "State Code cannot exceed 10 characters.")]
    public string? State_Code { get; set; }

    /// <summary>
    /// Active Status flag
    /// </summary>
    public bool State_IsActive { get; set; } = true;
}

/// <summary>
/// Model representing state data returned from SP_State_GetAll stored procedure.
/// </summary>
public class StateListModel
{
    /// <summary>
    /// State Identifier
    /// </summary>
    public int State_Id { get; set; }

    /// <summary>
    /// State Name
    /// </summary>
    public string State_Name { get; set; } = string.Empty;

    /// <summary>
    /// State Code (e.g., MH, GJ, KA, DL)
    /// </summary>
    public string State_Code { get; set; } = string.Empty;

    /// <summary>
    /// Active Status
    /// </summary>
    public bool State_IsActive { get; set; }
}

/// <summary>
/// Query filter parameters for SP_State_GetAll stored procedure.
/// </summary>
public class StateFilterDto
{
    /// <summary>
    /// Search term matching State Name or State Code
    /// </summary>
    public string? Search { get; set; } = string.Empty;

    /// <summary>
    /// Filter by Active Status (1 = Active, 0 = Inactive, null = All)
    /// </summary>
    public bool? IsActive { get; set; }
}
