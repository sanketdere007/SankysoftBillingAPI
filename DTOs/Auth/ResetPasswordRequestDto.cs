namespace Billing_Software_Api.DTOs.Auth;

/// <summary>
/// Data transfer object for resetting user password with verification token.
/// </summary>
public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
