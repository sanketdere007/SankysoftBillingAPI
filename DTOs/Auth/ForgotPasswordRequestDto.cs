namespace Billing_Software_Api.DTOs.Auth;

/// <summary>
/// Data transfer object for initiating password reset workflow.
/// </summary>
public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}
