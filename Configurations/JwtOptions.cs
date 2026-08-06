namespace Billing_Software_Api.Configurations;

/// <summary>
/// Strongly-typed configuration options for JSON Web Token (JWT) authentication.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "BillingSoftwareAPI";
    public string Audience { get; set; } = "BillingSoftwareClients";
    public int ExpiryInMinutes { get; set; } = 60;
    public int RefreshTokenExpiryInDays { get; set; } = 7;
}
