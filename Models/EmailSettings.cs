namespace Billing_Software_Api.Models;

/// <summary>
/// Single configuration section for Gmail SMTP bulk email sending.
/// Change Gmail credentials, subject, body, PDF path, and delay here (or in appsettings.json).
/// </summary>
public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    /// <summary>Gmail SMTP host. Do not change unless Gmail SMTP settings change.</summary>
    public string SmtpHost { get; set; } = "smtp.gmail.com";

    /// <summary>Gmail SMTP port using STARTTLS.</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>Gmail address used as the From address and SMTP username.</summary>
    public string GmailAddress { get; set; } = "sanketdere51@gmail.com";

    /// <summary>
    /// 16-character Gmail App Password (not the normal Gmail password).
    /// Spaces are stripped automatically. Never returned in API responses or logs.
    /// </summary>
    public string AppPassword { get; set; } = "vbnsrpykcjwjncec";

    /// <summary>Hardcoded email subject sent to every recipient.</summary>
    public string Subject { get; set; } = "Application: Flutter Mobile App Developer";

    /// <summary>Hardcoded email body sent to every recipient.</summary>
    public string Body { get; set; } = """
        Dear Sir/Madam,

        I hope you are doing well.

        I am writing to apply for a job opportunity at your company. I have 2+ years of experience in Flutter development, along with knowledge of .NET and SQL. I have worked on real-time projects including mobile and web applications, and I am passionate about building efficient and user-friendly apps.

        Please find my resume attached for your review. I would appreciate the opportunity to discuss my application further.

        Thank you for your time and consideration.

        Best regards,
        Sanket Dere
        Pune
        8411837139
        """;

    /// <summary>
    /// Hardcoded PDF attachment path. Relative paths are resolved from the application content root.
    /// The same file is attached to every email.
    /// </summary>
    public string PdfPath { get; set; } = @"D:\SANKET_DERE_Software_Developer.pdf";

    /// <summary>Delay in milliseconds between emails. Use 0 for fastest sending.</summary>
    public int DelayBetweenEmailsMs { get; set; } = 0;

    /// <summary>Maximum unique recipients allowed in a single API request.</summary>
    public int MaxRecipients { get; set; } = 100;
}
