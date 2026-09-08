using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RouteController : ControllerBase
{
    private readonly IRouteRepository _routeRepository;
    private readonly ILogger<RouteController> _logger;

    public RouteController(IRouteRepository routeRepository, ILogger<RouteController> logger)
    {
        _routeRepository = routeRepository ?? throw new ArgumentNullException(nameof(routeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("InsertorUpdateRoute")]
    [ProducesResponseType(typeof(ApiResponse<RouteSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RouteSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertorUpdateRoute([FromBody] RouteModel route, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<RouteSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new RouteSaveResult { Status = false, Message = "Validation failed.", Route_Id = route.Route_Id }));
        }

        var result = await _routeRepository.SaveRouteAsync(route, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("GetAllRoutes")]
    [ProducesResponseType(typeof(ApiResponse<List<RouteListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<RouteListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllRoutes([FromQuery] RouteFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _routeRepository.GetAllRoutesAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
