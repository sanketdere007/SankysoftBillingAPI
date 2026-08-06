namespace Billing_Software_Api.Models;

/// <summary>
/// Standard unified API response wrapper for all Web API endpoints.
/// </summary>
/// <typeparam name="T">Payload data type</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation succeeded (true) or failed (false).
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Human-readable message describing the outcome.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Data payload returned on successful operations.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Sanitized error description or validation details if the operation failed.
    /// </summary>
    public string? Error { get; set; }

    public static ApiResponse<T> SuccessResult(T? data, string message = "Operation completed successfully.")
    {
        return new ApiResponse<T>
        {
            Status = true,
            Message = message,
            Data = data,
            Error = null
        };
    }

    public static ApiResponse<T> FailureResult(string message, string? error = null, T? data = default)
    {
        return new ApiResponse<T>
        {
            Status = false,
            Message = message,
            Data = data,
            Error = error
        };
    }
}

/// <summary>
/// Non-generic API response wrapper for operations that do not return a data payload.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Success(string message = "Operation completed successfully.")
    {
        return new ApiResponse
        {
            Status = true,
            Message = message,
            Data = null,
            Error = null
        };
    }

    public static ApiResponse Failure(string message, string? error = null)
    {
        return new ApiResponse
        {
            Status = false,
            Message = message,
            Data = null,
            Error = error
        };
    }
}
