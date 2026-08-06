namespace Billing_Software_Api.Configurations;

/// <summary>
/// Strongly-typed configuration options for Cross-Origin Resource Sharing (CORS).
/// Specifically configured for Flutter Web and cross-origin desktop/mobile web views.
/// </summary>
public class CorsOptions
{
    public const string SectionName = "CorsSettings";

    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public bool AllowAnyOrigin { get; set; } = true;
    public bool AllowCredentials { get; set; } = false;
    public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS" };
    public string[] AllowedHeaders { get; set; } = new[] { "Authorization", "Content-Type", "Accept", "X-Requested-With", "Origin" };
    public string[] ExposedHeaders { get; set; } = new[] { "X-Pagination", "Content-Disposition" };
    public int PreflightMaxAgeSeconds { get; set; } = 3600;
}
