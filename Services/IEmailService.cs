using Billing_Software_Api.Models;

namespace Billing_Software_Api.Services;

public interface IEmailService
{
    Task<SendEmailResponse> SendBulkAsync(SendEmailRequest request, CancellationToken cancellationToken = default);
}
