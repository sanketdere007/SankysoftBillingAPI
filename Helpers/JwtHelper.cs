using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Billing_Software_Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Billing_Software_Api.Helpers;

/// <summary>
/// Interface for JWT token generation and validation helper.
/// </summary>
public interface IJwtHelper
{
    /// <summary>
    /// Generates a signed JWT access token for an authenticated employee.
    /// </summary>
    (string Token, DateTime Expiration) GenerateToken(EmployeeModel employee);

    /// <summary>
    /// Hashes a plaintext password securely.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies whether the provided plaintext password matches the stored hash.
    /// </summary>
    bool VerifyPassword(string password, string? storedHash);
}

/// <summary>
/// Production-grade JWT token generator and password security helper.
/// </summary>
public class JwtHelper : IJwtHelper
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryInMinutes;

    public JwtHelper(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        _secretKey = _configuration["JwtSettings:SecretKey"] 
            ?? "BillingSoftwareSuperSecretKeyForJwtTokenGeneration2026!#SecureKey";
        _issuer = _configuration["JwtSettings:Issuer"] ?? "BillingSoftwareAPI";
        _audience = _configuration["JwtSettings:Audience"] ?? "BillingSoftwareClients";

        if (!int.TryParse(_configuration["JwtSettings:ExpiryInMinutes"], out _expiryInMinutes))
        {
            _expiryInMinutes = 10080;
        }
    }

    /// <summary>
    /// Generates a signed JWT Bearer token with full employee claims.
    /// </summary>
    public (string Token, DateTime Expiration) GenerateToken(EmployeeModel employee)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);
        var expiration = DateTime.UtcNow.AddMinutes(_expiryInMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.Emp_Id.ToString()),
            new(ClaimTypes.Name, employee.Emp_UserName),
            new(ClaimTypes.GivenName, employee.Emp_FirstName),
            new(ClaimTypes.Surname, employee.Emp_LastName ?? string.Empty),
            new(ClaimTypes.Email, employee.Emp_Email ?? string.Empty),
            new(ClaimTypes.MobilePhone, employee.Emp_MobileNumber),
            new(ClaimTypes.Role, string.IsNullOrWhiteSpace(employee.Emp_Role) ? "Employee" : employee.Emp_Role),
            new("Department", employee.Emp_Department ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (employee.Emp_BranchId.HasValue)
        {
            claims.Add(new Claim("BranchId", employee.Emp_BranchId.Value.ToString()));
        }

        if (employee.Emp_CompId.HasValue)
        {
            claims.Add(new Claim("CompanyId", employee.Emp_CompId.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return (tokenHandler.WriteToken(token), expiration);
    }

    /// <summary>
    /// Securely hashes a plaintext password using BCrypt with salt.
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);
    }

    /// <summary>
    /// Verifies a plaintext password against a stored BCrypt or SHA256/plain hash.
    /// </summary>
    public bool VerifyPassword(string password, string? storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        var cleanHash = storedHash.Trim();
        var cleanPassword = password.Trim();

        // 1. Direct Plaintext match (for testing / unhashed records)
        if (string.Equals(password, cleanHash, StringComparison.Ordinal) ||
            string.Equals(cleanPassword, cleanHash, StringComparison.Ordinal))
        {
            return true;
        }

        // 2. BCrypt verification ($2a$, $2b$, $2y$)
        if (cleanHash.StartsWith("$2a$") || cleanHash.StartsWith("$2b$") || cleanHash.StartsWith("$2y$"))
        {
            try
            {
                if (BCrypt.Net.BCrypt.Verify(password, cleanHash) || BCrypt.Net.BCrypt.Verify(cleanPassword, cleanHash))
                {
                    return true;
                }
            }
            catch
            {
                // Continue to check other hashing mechanisms if BCrypt format was invalid/corrupted
            }
        }

        // Remove optional '0x' hex prefix from SQL HASHBYTES
        var hexHash = cleanHash.StartsWith("0x", StringComparison.OrdinalIgnoreCase) 
            ? cleanHash.Substring(2) 
            : cleanHash;

        // 3. SHA-256 verification (64 hex characters or 44 Base64 characters)
        try
        {
            using var sha256 = SHA256.Create();

            // Check UTF-8 / ASCII hex (matches SQL: CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', 'password'), 2))
            var utf8Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(cleanPassword));
            var utf8Hex = Convert.ToHexString(utf8Bytes);
            if (string.Equals(utf8Hex, hexHash, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Also check original un-trimmed password UTF-8 hex
            var rawUtf8Bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var rawUtf8Hex = Convert.ToHexString(rawUtf8Bytes);
            if (string.Equals(rawUtf8Hex, hexHash, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check Unicode / UTF-16LE hex (matches SQL: CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', N'password'), 2))
            var unicodeBytes = sha256.ComputeHash(Encoding.Unicode.GetBytes(cleanPassword));
            var unicodeHex = Convert.ToHexString(unicodeBytes);
            if (string.Equals(unicodeHex, hexHash, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check Base64 SHA-256
            var utf8Base64 = Convert.ToBase64String(utf8Bytes);
            if (string.Equals(utf8Base64, cleanHash, StringComparison.Ordinal))
            {
                return true;
            }
        }
        catch
        {
            // Ignore and continue
        }

        // 4. MD5 fallback (32 hex characters)
        try
        {
            using var md5 = MD5.Create();
            var md5Bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(cleanPassword));
            var md5Hex = Convert.ToHexString(md5Bytes);
            if (string.Equals(md5Hex, hexHash, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        catch
        {
            // Ignore
        }

        return false;
    }
}
