using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Authentication API Controller managing employee login and JWT token issuance.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthRepository authRepository, ILogger<AuthController> logger)
    {
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// API 2: Employee Login
    /// Authenticates an employee using Emp_UserName and Password against Emp_PasswordHash in SQL Server.
    /// Returns a signed JWT Token and employee profile details upon success.
    /// </summary>
    /// <param name="loginRequest">Employee login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT Bearer token and employee details</returns>
    [HttpPost("Login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<LoginResponse>.FailureResult(
                message: "Validation failed.",
                error: errors));
        }

        var result = await _authRepository.LoginAsync(loginRequest, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return Unauthorized(result);
    }
}
