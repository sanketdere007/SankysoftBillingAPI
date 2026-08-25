using System.Text.Json.Serialization;

namespace Billing_Software_Api.Models;

/// <summary>
/// Bulk email send summary returned by POST /api/email/send.
/// </summary>
public class SendEmailResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("sent")]
    public int Sent { get; set; }

    [JsonPropertyName("failed")]
    public int Failed { get; set; }

    [JsonPropertyName("results")]
    public List<EmailSendResultItem> Results { get; set; } = [];

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}

/// <summary>
/// Per-recipient send outcome.
/// </summary>
public class EmailSendResultItem
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
