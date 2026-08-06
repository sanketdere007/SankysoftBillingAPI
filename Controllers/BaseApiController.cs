using Billing_Software_Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Base API controller providing standardized response handling for derived controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Translates an ApiResponse&lt;T&gt; into the corresponding ActionResult with appropriate HTTP status code.
    /// </summary>
    protected IActionResult HandleResult<T>(ApiResponse<T> result)
    {
        return result.StatusCode switch
        {
            StatusCodes.Status200OK => Ok(result),
            StatusCodes.Status201Created => StatusCode(StatusCodes.Status201Created, result),
            StatusCodes.Status204NoContent => NoContent(),
            StatusCodes.Status400BadRequest => BadRequest(result),
            StatusCodes.Status401Unauthorized => Unauthorized(result),
            StatusCodes.Status403Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
            StatusCodes.Status404NotFound => NotFound(result),
            StatusCodes.Status409Conflict => Conflict(result),
            StatusCodes.Status422UnprocessableEntity => UnprocessableEntity(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    /// <summary>
    /// Translates a non-generic ApiResponse into the corresponding ActionResult with appropriate HTTP status code.
    /// </summary>
    protected IActionResult HandleResult(ApiResponse result)
    {
        return result.StatusCode switch
        {
            StatusCodes.Status200OK => Ok(result),
            StatusCodes.Status201Created => StatusCode(StatusCodes.Status201Created, result),
            StatusCodes.Status204NoContent => NoContent(),
            StatusCodes.Status400BadRequest => BadRequest(result),
            StatusCodes.Status401Unauthorized => Unauthorized(result),
            StatusCodes.Status403Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
            StatusCodes.Status404NotFound => NotFound(result),
            StatusCodes.Status409Conflict => Conflict(result),
            StatusCodes.Status422UnprocessableEntity => UnprocessableEntity(result),
            _ => StatusCode(result.StatusCode, result)
        };
    }

    /// <summary>
    /// Translates a PagedResponse&lt;T&gt; into the corresponding ActionResult.
    /// </summary>
    protected IActionResult HandlePagedResult<T>(PagedResponse<T> result)
    {
        return Ok(result);
    }
}
