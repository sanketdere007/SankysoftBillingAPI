using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

/// <summary>
/// Model representing branch data returned from SP_Branch_GetAll stored procedure.
/// </summary>
public class BranchListModel
{
    /// <summary>
    /// Branch Primary Key Identifier
    /// </summary>
    public int Branch_Id { get; set; }

    /// <summary>
    /// Associated Company Identifier
    /// </summary>
    public int Branch_CompId { get; set; }

    /// <summary>
    /// Associated Company Name (joined from tbl_Company)
    /// </summary>
    public string Branch_CompName { get; set; } = string.Empty;

    /// <summary>
    /// Branch Name
    /// </summary>
    public string Branch_Name { get; set; } = string.Empty;

    /// <summary>
    /// Primary Contact Person Name
    /// </summary>
    public string? Branch_ContactPerson { get; set; }

    /// <summary>
    /// Primary Mobile Number
    /// </summary>
    public string? Branch_MobileNo { get; set; }

    /// <summary>
    /// Alternate Contact Mobile Number
    /// </summary>
    public string? Branch_AlternateMobileNo { get; set; }

    /// <summary>
    /// Branch Email Address
    /// </summary>
    public string? Branch_Email { get; set; }

    /// <summary>
    /// Branch GST Number
    /// </summary>
    public string? Branch_GSTNo { get; set; }

    /// <summary>
    /// Street Address / Building Details
    /// </summary>
    public string? Branch_Address { get; set; }

    /// <summary>
    /// Area / Locality
    /// </summary>
    public string? Branch_Area { get; set; }

    /// <summary>
    /// City Name
    /// </summary>
    public string? Branch_City { get; set; }

    /// <summary>
    /// State Name
    /// </summary>
    public string? Branch_State { get; set; }

    /// <summary>
    /// Postal Pincode
    /// </summary>
    public string? Branch_Pincode { get; set; }

    /// <summary>
    /// Country Name
    /// </summary>
    public string? Branch_Country { get; set; }

    /// <summary>
    /// Active Status Flag
    /// </summary>
    public bool Branch_IsActive { get; set; }

    /// <summary>
    /// User ID of Creator
    /// </summary>
    public int? Branch_CreatedBy { get; set; }

    /// <summary>
    /// Record Creation Timestamp
    /// </summary>
    public DateTime? Branch_CreatedDate { get; set; }

    /// <summary>
    /// User ID of Modifier
    /// </summary>
    public int? Branch_ModifiedBy { get; set; }

    /// <summary>
    /// Record Last Modification Timestamp
    /// </summary>
    public DateTime? Branch_ModifiedDate { get; set; }
}

/// <summary>
/// Query filter parameters for SP_Branch_GetAll stored procedure.
/// </summary>
public class BranchFilterDto
{
    /// <summary>
    /// Filter by Company ID (0 or null = All Companies)
    /// </summary>
    public int? Branch_CompId { get; set; }

    /// <summary>
    /// Alias for Branch_CompId to support ?compId=1 query param seamlessly.
    /// </summary>
    public int? CompId
    {
        get => Branch_CompId;
        set => Branch_CompId = value;
    }

    /// <summary>
    /// Filter by Active Status (1 = Active, 0 = Inactive, null = All)
    /// </summary>
    public bool? Branch_IsActive { get; set; }

    /// <summary>
    /// Alias for Branch_IsActive to support ?isActive=true query param seamlessly.
    /// </summary>
    public bool? IsActive
    {
        get => Branch_IsActive;
        set => Branch_IsActive = value;
    }
}
