using System.Net.Mail;
using Billing_Software_Api.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Billing_Software_Api.Services;

/// <summary>
/// Sends the same hardcoded email (subject, body, PDF) to many recipients over Gmail SMTP.
/// Emails are sent sequentially. A failure for one recipient does not stop the rest.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        IHostEnvironment environment,
        ILogger<EmailService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SendEmailResponse> SendBulkAsync(SendEmailRequest request, CancellationToken cancellationToken = default)
    {
        if (request?.Emails == null || request.Emails.Count == 0)
        {
            return FailedResponse("The emails list is required and cannot be empty.");
        }

        var recipients = DeduplicateEmails(request.Emails);

        if (recipients.Count == 0)
        {
            return FailedResponse("No email addresses were provided after removing empty values.");
        }

        if (recipients.Count > _settings.MaxRecipients)
        {
            return FailedResponse($"A maximum of {_settings.MaxRecipients} unique email addresses is allowed per request.");
        }

        var credentialError = ValidateSmtpCredentials();
        if (credentialError != null)
        {
            return FailedResponse(credentialError);
        }

        var pdfPath = ResolvePdfPath(_settings.PdfPath);
        if (!File.Exists(pdfPath))
        {
            _logger.LogWarning("PDF attachment was not found at {PdfPath}. Bulk send was not started.", pdfPath);
            return FailedResponse($"PDF file not found at '{pdfPath}'. Sending was not started.");
        }

        var results = new List<EmailSendResultItem>(recipients.Count);
        var sent = 0;
        var failed = 0;

        var pdfBytes = await File.ReadAllBytesAsync(pdfPath, cancellationToken);
        var pdfFileName = Path.GetFileName(pdfPath);

        using var smtp = CreateSmtpClient();

        try
        {
            await ConnectAndAuthenticateAsync(smtp, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            foreach (var email in recipients)
            {
                results.Add(FailedItem(email, "Request cancelled"));
            }

            return new SendEmailResponse
            {
                Success = true,
                Total = recipients.Count,
                Sent = 0,
                Failed = recipients.Count,
                Results = results
            };
        }
        catch (AuthenticationException ex)
        {
            var error = Sanitize(
                "Gmail authentication failed. Use a 16-character App Password (not the normal Gmail password). " +
                ex.Message);
            _logger.LogError(ex, "Gmail SMTP authentication failed. Password is not logged.");

            foreach (var email in recipients)
            {
                results.Add(FailedItem(email, error));
            }

            return new SendEmailResponse
            {
                Success = true,
                Total = recipients.Count,
                Sent = 0,
                Failed = recipients.Count,
                Results = results
            };
        }
        catch (Exception ex)
        {
            var error = Sanitize($"SMTP connection or authentication failed. {ex.Message}");
            _logger.LogError(ex, "Gmail SMTP connect/authenticate failed. Password is not logged.");

            foreach (var email in recipients)
            {
                results.Add(FailedItem(email, error));
            }

            return new SendEmailResponse
            {
                Success = true,
                Total = recipients.Count,
                Sent = 0,
                Failed = recipients.Count,
                Results = results
            };
        }

        try
        {
            for (var i = 0; i < recipients.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    for (var j = i; j < recipients.Count; j++)
                    {
                        failed++;
                        results.Add(FailedItem(recipients[j], "Request cancelled"));
                    }

                    break;
                }

                var email = recipients[i];

                if (!IsValidEmail(email))
                {
                    failed++;
                    results.Add(FailedItem(email, "Invalid email address"));
                }
                else
                {
                    try
                    {
                        if (!smtp.IsConnected || !smtp.IsAuthenticated)
                        {
                            await ConnectAndAuthenticateAsync(smtp, cancellationToken);
                        }

                        using var message = BuildMessage(email, pdfBytes, pdfFileName);
                        await smtp.SendAsync(message, cancellationToken);

                        sent++;
                        results.Add(new EmailSendResultItem
                        {
                            Email = email,
                            Status = "Sent"
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        failed++;
                        results.Add(FailedItem(email, "Request cancelled"));
                        for (var j = i + 1; j < recipients.Count; j++)
                        {
                            failed++;
                            results.Add(FailedItem(recipients[j], "Request cancelled"));
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        var error = Sanitize(ex.Message);
                        _logger.LogWarning("Failed to send email to {Email}. {Error}", email, error);
                        results.Add(FailedItem(email, error));
                    }
                }

                if (i < recipients.Count - 1 && _settings.DelayBetweenEmailsMs > 0)
                {
                    try
                    {
                        await Task.Delay(_settings.DelayBetweenEmailsMs, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        for (var j = i + 1; j < recipients.Count; j++)
                        {
                            failed++;
                            results.Add(FailedItem(recipients[j], "Request cancelled"));
                        }

                        break;
                    }
                }
            }
        }
        finally
        {
            if (smtp.IsConnected)
            {
                await smtp.DisconnectAsync(true, CancellationToken.None);
            }
        }

        return new SendEmailResponse
        {
            Success = true,
            Total = recipients.Count,
            Sent = sent,
            Failed = failed,
            Results = results
        };
    }

    private static SmtpClient CreateSmtpClient()
    {
        return new SmtpClient
        {
            Timeout = 15000,
            CheckCertificateRevocation = false
        };
    }

    private async Task ConnectAndAuthenticateAsync(SmtpClient smtp, CancellationToken cancellationToken)
    {
        if (!smtp.IsConnected)
        {
            await smtp.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                SecureSocketOptions.StartTls,
                cancellationToken);
        }

        // Gmail advertises XOAUTH2 first; skip it so App Password auth does not wait on a failed OAuth attempt.
        smtp.AuthenticationMechanisms.Remove("XOAUTH2");

        if (!smtp.IsAuthenticated)
        {
            await smtp.AuthenticateAsync(_settings.GmailAddress, GetAppPassword(), cancellationToken);
        }
    }

    private MimeMessage BuildMessage(string toEmail, byte[] pdfBytes, string pdfFileName)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sanket Dere", _settings.GmailAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = _settings.Subject;

        var builder = new BodyBuilder
        {
            TextBody = _settings.Body
        };
        builder.Attachments.Add(pdfFileName, pdfBytes, new ContentType("application", "pdf"));
        message.Body = builder.ToMessageBody();
        return message;
    }

    private string ResolvePdfPath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.Combine(_environment.ContentRootPath, "Attachments", "invoice.pdf");
        }

        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, configuredPath));
    }

    private static List<string> DeduplicateEmails(IEnumerable<string> emails)
    {
        var unique = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in emails)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            var email = raw.Trim();
            if (seen.Add(email))
            {
                unique.Add(email);
            }
        }

        return unique;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var parsed = new MailAddress(email);
            return string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase)
                   && parsed.Address.Contains('@');
        }
        catch
        {
            return false;
        }
    }

    private string? ValidateSmtpCredentials()
    {
        var address = _settings.GmailAddress?.Trim() ?? string.Empty;
        var password = GetAppPassword();

        if (string.IsNullOrWhiteSpace(address)
            || address.Contains("your-email", StringComparison.OrdinalIgnoreCase))
        {
            return "GmailAddress is not configured. Set EmailSettings:GmailAddress in appsettings.json and restart the API.";
        }

        if (string.IsNullOrWhiteSpace(password)
            || password.Contains("your-16-char-app-password", StringComparison.OrdinalIgnoreCase)
            || password.Contains("changeme", StringComparison.OrdinalIgnoreCase))
        {
            return "Gmail App Password is not configured. Set EmailSettings:AppPassword in appsettings.json (16-character App Password, not the normal Gmail password) and restart the API.";
        }

        if (password.Length < 16)
        {
            return "Gmail App Password looks invalid. Generate a 16-character App Password at https://myaccount.google.com/apppasswords and update EmailSettings:AppPassword.";
        }

        return null;
    }

    private string GetAppPassword()
    {
        return (_settings.AppPassword ?? string.Empty).Replace(" ", string.Empty);
    }

    private string Sanitize(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return "SMTP error";
        }

        var password = GetAppPassword();
        if (!string.IsNullOrEmpty(password) && message.Contains(password, StringComparison.Ordinal))
        {
            message = message.Replace(password, "***", StringComparison.Ordinal);
        }

        if (!string.IsNullOrEmpty(_settings.AppPassword)
            && message.Contains(_settings.AppPassword, StringComparison.Ordinal))
        {
            message = message.Replace(_settings.AppPassword, "***", StringComparison.Ordinal);
        }

        return message;
    }

    private static EmailSendResultItem FailedItem(string email, string error)
    {
        return new EmailSendResultItem
        {
            Email = email,
            Status = "Failed",
            Error = error
        };
    }

    private static SendEmailResponse FailedResponse(string error)
    {
        return new SendEmailResponse
        {
            Success = false,
            Total = 0,
            Sent = 0,
            Failed = 0,
            Results = [],
            Error = error
        };
    }
}
