namespace Billing_Software_Api.DTOs.Auth;

/// <summary>
/// Data transfer object for user login credentials.
/// </summary>
public class LoginRequestDto
{
    public string EmailOrUsername { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
