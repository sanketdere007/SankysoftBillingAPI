namespace Billing_Software_Api.DTOs.Auth;

/// <summary>
/// Data transfer object for renewing access token with a valid refresh token.
/// </summary>
public class RefreshTokenRequestDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
