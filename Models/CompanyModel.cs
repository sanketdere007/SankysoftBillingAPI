using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

/// <summary>
/// Model representing company data returned from SP_Company_GetAll stored procedure.
/// </summary>
public class CompanyListModel
{
    /// <summary>
    /// Company Primary Key Identifier
    /// </summary>
    public int Comp_Id { get; set; }

    /// <summary>
    /// Company Name
    /// </summary>
    public string Comp_Name { get; set; } = string.Empty;

    /// <summary>
    /// Primary Contact Person Name
    /// </summary>
    public string? Comp_ContactPerson { get; set; }

    /// <summary>
    /// Primary Mobile Number
    /// </summary>
    public string? Comp_MobileNo { get; set; }

    /// <summary>
    /// Alternate Contact Mobile Number
    /// </summary>
    public string? Comp_AlternateMobileNo { get; set; }

    /// <summary>
    /// Company Email Address
    /// </summary>
    public string? Comp_Email { get; set; }

    /// <summary>
    /// Official Website URL
    /// </summary>
    public string? Comp_Website { get; set; }

    /// <summary>
    /// GST Number
    /// </summary>
    public string? Comp_GSTNo { get; set; }

    /// <summary>
    /// PAN Card Number
    /// </summary>
    public string? Comp_PANNo { get; set; }

    /// <summary>
    /// Street Address / Building Details
    /// </summary>
    public string? Comp_Address { get; set; }

    /// <summary>
    /// Area / Locality
    /// </summary>
    public string? Comp_Area { get; set; }

    /// <summary>
    /// City Name
    /// </summary>
    public string? Comp_City { get; set; }

    /// <summary>
    /// State Name
    /// </summary>
    public string? Comp_State { get; set; }

    /// <summary>
    /// Postal Pincode
    /// </summary>
    public string? Comp_Pincode { get; set; }

    /// <summary>
    /// Country Name
    /// </summary>
    public string? Comp_Country { get; set; }

    /// <summary>
    /// Company Logo URL or File Path
    /// </summary>
    public string? Comp_Logo { get; set; }

    /// <summary>
    /// Active Status Flag
    /// </summary>
    public bool Comp_IsActive { get; set; }

    /// <summary>
    /// User ID of Creator
    /// </summary>
    public int? Comp_CreatedBy { get; set; }

    /// <summary>
    /// Record Creation Timestamp
    /// </summary>
    public DateTime? Comp_CreatedDate { get; set; }

    /// <summary>
    /// User ID of Modifier
    /// </summary>
    public int? Comp_ModifiedBy { get; set; }

    /// <summary>
    /// Record Last Modification Timestamp
    /// </summary>
    public DateTime? Comp_ModifiedDate { get; set; }
}

/// <summary>
/// Query filter parameters for SP_Company_GetAll stored procedure.
/// </summary>
public class CompanyFilterDto
{
    /// <summary>
    /// Filter by Active Status (1 = Active, 0 = Inactive, null = All)
    /// </summary>
    public bool? Comp_IsActive { get; set; }

    /// <summary>
    /// Alias for Comp_IsActive to support ?isActive=true query param seamlessly.
    /// </summary>
    public bool? IsActive
    {
        get => Comp_IsActive;
        set => Comp_IsActive = value;
    }
}
