using System.Security.Claims;

namespace Billing_Software_Api.Helpers;

/// <summary>
/// Platform-independent JWT token contract for authentication across all client applications.
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(int userId, string username, string role, IEnumerable<Claim>? additionalClaims = null);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
}
