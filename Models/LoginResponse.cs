namespace Billing_Software_Api.Models;

/// <summary>
/// Response returned upon successful employee authentication containing JWT token and profile info.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT Bearer access token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration timestamp (UTC).
    /// </summary>
    public DateTime Expiration { get; set; }

    /// <summary>
    /// Authenticated Employee ID.
    /// </summary>
    public int Emp_Id { get; set; }

    /// <summary>
    /// Employee First Name.
    /// </summary>
    public string Emp_FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Employee Middle Name.
    /// </summary>
    public string? Emp_MiddleName { get; set; }

    /// <summary>
    /// Employee Last Name.
    /// </summary>
    public string? Emp_LastName { get; set; }

    /// <summary>
    /// Employee Email.
    /// </summary>
    public string? Emp_Email { get; set; }

    /// <summary>
    /// Employee Mobile Number.
    /// </summary>
    public string Emp_MobileNumber { get; set; } = string.Empty;

    /// <summary>
    /// Employee Username.
    /// </summary>
    public string Emp_UserName { get; set; } = string.Empty;

    /// <summary>
    /// Employee Gender.
    /// </summary>
    public string? Emp_Gender { get; set; }

    /// <summary>
    /// Employee Role (e.g., Admin, Manager, Employee).
    /// </summary>
    public string Emp_Role { get; set; } = string.Empty;

    /// <summary>
    /// Employee Branch Identifier.
    /// </summary>
    public int? Emp_BranchId { get; set; }

    /// <summary>
    /// Employee Company Identifier.
    /// </summary>
    public int? Emp_CompId { get; set; }

    /// <summary>
    /// Employee Department.
    /// </summary>
    public string? Emp_Department { get; set; }

    /// <summary>
    /// Employee Designation.
    /// </summary>
    public string? Emp_Designation { get; set; }

    /// <summary>
    /// Employee Joining Date.
    /// </summary>
    public DateTime? Emp_JoiningDate { get; set; }

    /// <summary>
    /// Employee Active Status.
    /// </summary>
    public bool Emp_IsActive { get; set; } = true;
}
