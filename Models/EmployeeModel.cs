using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Billing_Software_Api.Models;

/// <summary>
/// Employee model representing employee data for CRUD operations and JSON serialization.
/// </summary>
public class EmployeeModel
{
    /// <summary>
    /// Employee Identifier (0 for Insert, > 0 for Update)
    /// </summary>
    public int Emp_Id { get; set; } = 0;

    /// <summary>
    /// Employee First Name (Required)
    /// </summary>
    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters.")]
    public string Emp_FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Employee Middle Name
    /// </summary>
    [StringLength(50, ErrorMessage = "Middle Name cannot exceed 50 characters.")]
    public string? Emp_MiddleName { get; set; }

    /// <summary>
    /// Employee Last Name
    /// </summary>
    [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters.")]
    public string? Emp_LastName { get; set; }

    /// <summary>
    /// Employee Gender (e.g. Male, Female, Other)
    /// </summary>
    [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters.")]
    public string? Emp_Gender { get; set; }

    /// <summary>
    /// Employee Email Address (Valid format required)
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid Email Format.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string? Emp_Email { get; set; }

    /// <summary>
    /// Employee 10-digit Mobile Number (Required)
    /// </summary>
    [Required(ErrorMessage = "Mobile Number is required.")]
    [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Mobile Number must be exactly 10 digits.")]
    public string Emp_MobileNumber { get; set; } = string.Empty;

    /// <summary>
    /// Employee Username for Login (Required)
    /// </summary>
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
    public string Emp_UserName { get; set; } = string.Empty;

    /// <summary>
    /// Plaintext password passed during creation or password update.
    /// Will be converted to Emp_PasswordHash before passing to DB.
    /// Ignored during JSON serialization for read queries.
    /// </summary>
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string? Emp_Password { get; set; }

    /// <summary>
    /// Secure Password Hash stored in Database.
    /// </summary>
    public string? Emp_PasswordHash { get; set; }

    /// <summary>
    /// Employee Role (e.g., Admin, Manager, Employee)
    /// </summary>
    public string Emp_Role { get; set; } = "Employee";

    /// <summary>
    /// Branch Identifier mapped to employee
    /// </summary>
    public int? Emp_BranchId { get; set; }

    /// <summary>
    /// Company Identifier mapped to employee
    /// </summary>
    public int? Emp_CompId { get; set; }

    /// <summary>
    /// Employee Department (e.g., Accounts, IT, Sales)
    /// </summary>
    public string? Emp_Department { get; set; }

    /// <summary>
    /// Employee Designation
    /// </summary>
    public string? Emp_Designation { get; set; }

    /// <summary>
    /// Employee Base Salary
    /// </summary>
    [Range(0, 10000000, ErrorMessage = "Salary must be a positive number.")]
    public decimal? Emp_Salary { get; set; }

    /// <summary>
    /// Street Address
    /// </summary>
    public string? Emp_Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string? Emp_City { get; set; }

    /// <summary>
    /// State
    /// </summary>
    public string? Emp_State { get; set; }

    /// <summary>
    /// Postal Pincode
    /// </summary>
    public string? Emp_Pincode { get; set; }

    /// <summary>
    /// Date of Birth
    /// </summary>
    public DateTime? Emp_DateOfBirth { get; set; }

    /// <summary>
    /// Date of Joining
    /// </summary>
    public DateTime? Emp_DateOfJoining { get; set; }

    /// <summary>
    /// Alias for Emp_DateOfJoining matching SQL column Emp_JoiningDate
    /// </summary>
    public DateTime? Emp_JoiningDate
    {
        get => Emp_DateOfJoining;
        set => Emp_DateOfJoining = value;
    }

    /// <summary>
    /// Active Status
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Alias for IsActive matching SQL column Emp_IsActive
    /// </summary>
    public bool Emp_IsActive
    {
        get => IsActive;
        set => IsActive = value;
    }

    /// <summary>
    /// Record Creation Timestamp
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Record Last Modified Timestamp
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
}
