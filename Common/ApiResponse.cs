using System.Text.Json.Serialization;

namespace Billing_Software_Api.Common;

/// <summary>
/// Standardized generic API response wrapper.
/// </summary>
/// <typeparam name="T">Type of data payload.</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Errors { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse()
    {
    }

    public ApiResponse(bool success, string message, int statusCode, T? data = default, IReadOnlyList<string>? errors = null)
    {
        Success = success;
        Message = message;
        StatusCode = statusCode;
        Data = data;
        Errors = errors;
    }

    public static ApiResponse<T> SuccessResult(T data, string message = "Request completed successfully.", int statusCode = 200)
    {
        return new ApiResponse<T>(true, message, statusCode, data);
    }

    public static ApiResponse<T> FailureResult(string message, IReadOnlyList<string>? errors = null, int statusCode = 400)
    {
        return new ApiResponse<T>(false, message, statusCode, default, errors);
    }

    public static ApiResponse<T> FailureResult(string message, string error, int statusCode = 400)
    {
        return new ApiResponse<T>(false, message, statusCode, default, new[] { error });
    }
}

/// <summary>
/// Standardized non-generic API response wrapper for operations without data payload.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Errors { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse()
    {
    }

    public ApiResponse(bool success, string message, int statusCode, IReadOnlyList<string>? errors = null)
    {
        Success = success;
        Message = message;
        StatusCode = statusCode;
        Errors = errors;
    }

    public static ApiResponse SuccessResult(string message = "Operation completed successfully.", int statusCode = 200)
    {
        return new ApiResponse(true, message, statusCode);
    }

    public static ApiResponse FailureResult(string message, IReadOnlyList<string>? errors = null, int statusCode = 400)
    {
        return new ApiResponse(false, message, statusCode, errors);
    }

    public static ApiResponse FailureResult(string message, string error, int statusCode = 400)
    {
        return new ApiResponse(false, message, statusCode, new[] { error });
    }
}
