using Billing_Software_Api.Models;
using Billing_Software_Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Sends a hardcoded invoice email with a PDF attachment to a list of recipients via Gmail SMTP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Send the same hardcoded email and PDF to each address in the request, sequentially.
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(SendEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SendEmailResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Send([FromBody] SendEmailRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _emailService.SendBulkAsync(request, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning("Bulk email send was not started: {Error}", result.Error);
            return BadRequest(result);
        }

        _logger.LogInformation(
            "Bulk email send finished. Total={Total}, Sent={Sent}, Failed={Failed}",
            result.Total,
            result.Sent,
            result.Failed);

        return Ok(result);
    }
}
