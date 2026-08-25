using System.Text.Json.Serialization;

namespace Billing_Software_Api.Models;

/// <summary>
/// Bulk email request. Accepts only a JSON list of recipient addresses.
/// </summary>
public class SendEmailRequest
{
    [JsonPropertyName("emails")]
    public List<string> Emails { get; set; } = [];
}
