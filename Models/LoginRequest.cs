using System.ComponentModel.DataAnnotations;

namespace Billing_Software_Api.Models;

/// <summary>
/// Request payload for employee authentication.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Employee Username (Required)
    /// </summary>
    [Required(ErrorMessage = "Username is required.")]
    public string Emp_UserName { get; set; } = string.Empty;

    /// <summary>
    /// Employee Plaintext Password (Required)
    /// </summary>
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}
