using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

/// <summary>
/// Customer model representing customer data for insert, update, and retrieval operations.
/// Mapped to SP_Customer_InsertOrUpdate stored procedure JSON payload.
/// </summary>
public class CustomerModel
{
    /// <summary>
    /// Customer ID (0 for Insert, > 0 for Update)
    /// </summary>
    public int Cust_Id { get; set; } = 0;

    /// <summary>
    /// Auto-generated Customer Code (e.g. CUST000001)
    /// </summary>
    public string? Cust_Code { get; set; }

    /// <summary>
    /// Customer Full Name
    /// </summary>
    [Required(ErrorMessage = "Customer Name is required.")]
    [StringLength(150, ErrorMessage = "Customer Name cannot exceed 150 characters.")]
    public string Cust_Name { get; set; } = string.Empty;

    /// <summary>
    /// Customer Company / Business Name
    /// </summary>
    [StringLength(150, ErrorMessage = "Company Name cannot exceed 150 characters.")]
    public string? Cust_CompanyName { get; set; }

    /// <summary>
    /// Customer Primary Mobile Number (Required)
    /// </summary>
    [Required(ErrorMessage = "Mobile Number is required.")]
    [StringLength(15, MinimumLength = 10, ErrorMessage = "Mobile Number must be between 10 and 15 digits.")]
    public string Cust_MobileNo { get; set; } = string.Empty;

    /// <summary>
    /// Customer Alternate Mobile Number
    /// </summary>
    [StringLength(15, ErrorMessage = "Alternate Mobile Number cannot exceed 15 digits.")]
    public string? Cust_AlternateMobileNo { get; set; }

    /// <summary>
    /// Customer Email Address
    /// </summary>
   /// [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
   /// [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string? Cust_Email { get; set; }

    /// <summary>
    /// Customer GST Identification Number
    /// </summary>
    [StringLength(20, ErrorMessage = "GST Number cannot exceed 20 characters.")]
    public string? Cust_GSTNo { get; set; }

    /// <summary>
    /// Customer PAN Card Number
    /// </summary>
    [StringLength(10, ErrorMessage = "PAN Number cannot exceed 10 characters.")]
    public string? Cust_PANNo { get; set; }

    /// <summary>
    /// Customer Street Address
    /// </summary>
    [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
    public string? Cust_Address { get; set; }

    /// <summary>
    /// Customer Area ID (references tbl_Area.Area_Id)
    /// </summary>
    public int? Cust_AreaId { get; set; }

    /// <summary>
    /// Customer City ID (references tbl_City.City_Id)
    /// </summary>
    public int? Cust_CityId { get; set; }

    /// <summary>
    /// Customer State ID (references tbl_State.State_Id)
    /// </summary>
    public int? Cust_StateId { get; set; }

    /// <summary>
    /// Postal Pincode
    /// </summary>
    [StringLength(10, ErrorMessage = "Pincode cannot exceed 10 characters.")]
    public string? Cust_Pincode { get; set; }

    /// <summary>
    /// Country (Defaults to 'India')
    /// </summary>
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters.")]
    public string Cust_Country { get; set; } = "India";

    /// <summary>
    /// Branch Identifier mapped to customer
    /// </summary>
    public int? Cust_BranchId { get; set; }

    /// <summary>
    /// Company Identifier mapped to customer
    /// </summary>
    public int? Cust_CompId { get; set; }

    /// <summary>
    /// Customer Active Status
    /// </summary>
    public bool Cust_IsActive { get; set; } = true;

    /// <summary>
    /// User ID of creator
    /// </summary>
    public int Cust_CreatedBy { get; set; } = 0;

    /// <summary>
    /// Record Creation Timestamp
    /// </summary>
    public DateTime? Cust_CreatedDate { get; set; }

    /// <summary>
    /// User ID of modifier
    /// </summary>
    public int Cust_ModifiedBy { get; set; } = 0;

    /// <summary>
    /// Record Modification Timestamp
    /// </summary>
    public DateTime? Cust_ModifiedDate { get; set; }
}

/// <summary>
/// Model representing customer data returned from SP_Customer_GetAll and SP_Customer_GetById stored procedures.
/// Includes joined / auto-populated master names (Area, City, State).
/// </summary>
public class CustomerListModel : CustomerModel
{
    /// <summary>
    /// Customer Area / Locality Name
    /// </summary>
    public string? Cust_Area { get; set; }

    /// <summary>
    /// City Name
    /// </summary>
    public string? Cust_City { get; set; }

    /// <summary>
    /// State Name
    /// </summary>
    public string? Cust_State { get; set; }
}

/// <summary>
/// Query filter parameters for SP_Customer_GetAll stored procedure: Search, AreaId, CityId, StateId, BranchId, IsActive
/// </summary>
public class CustomerFilterDto
{
    /// <summary>
    /// Search term matching Cust_Code, Cust_Name, Cust_CompanyName, Cust_MobileNo, Cust_AlternateMobileNo, Cust_Email, Cust_GSTNo, Cust_PANNo
    /// </summary>
    public string? Search { get; set; } = string.Empty;

    /// <summary>
    /// Filter by Area ID or Area Name (NVARCHAR(250), default "0" for all)
    /// </summary>
    public string? AreaId { get; set; } = "0";

    /// <summary>
    /// Filter by City ID or City Name (NVARCHAR(100), default "0" for all)
    /// </summary>
    public string? CityId { get; set; } = "0";

    /// <summary>
    /// Filter by State ID or State Name (NVARCHAR(100), default "0" for all)
    /// </summary>
    public string? StateId { get; set; } = "0";

    /// <summary>
    /// Filter by Branch ID (INT, default 0 for all)
    /// </summary>
    public int? BranchId { get; set; } = 0;
    public int? CompId { get; set; } = 0;

    /// <summary>
    /// Filter by Active Status (1 = Active, 0 = Inactive, null = All)
    /// </summary>
    public bool? IsActive { get; set; } = true;
}

