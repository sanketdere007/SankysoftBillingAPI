using Billing_Software_Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Health check and system status endpoint.
/// </summary>
public class HealthController : BaseApiController
{
    /// <summary>
    /// Returns the operational health status and server timestamp.
    /// </summary>
    [HttpGet]
    public IActionResult Check()
    {
        var healthData = new
        {
            Status = "Healthy",
            Service = "Billing Software API",
            Version = "1.0.0",
            ServerTimeUtc = DateTime.UtcNow
        };

        return HandleResult(ApiResponse<object>.SuccessResult(healthData, "API is running and operational."));
    }
}
