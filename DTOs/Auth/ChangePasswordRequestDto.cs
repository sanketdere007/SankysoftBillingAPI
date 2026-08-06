namespace Billing_Software_Api.DTOs.Auth;

/// <summary>
/// Data transfer object for authenticated user password changes.
/// </summary>
public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
