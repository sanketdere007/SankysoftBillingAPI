namespace Billing_Software_Api.Common;

/// <summary>
/// Result object representing the outcome of an operation.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public IReadOnlyList<string> Errors { get; }

    protected Result(bool isSuccess, string message, IReadOnlyList<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Errors = errors ?? Array.Empty<string>();
    }

    public static Result Success(string message = "Operation succeeded.") => new(true, message);
    public static Result Failure(string message, IReadOnlyList<string>? errors = null) => new(false, message, errors);
    public static Result Failure(string message, string error) => new(false, message, new[] { error });
}

/// <summary>
/// Generic result object representing the outcome of an operation with a value.
/// </summary>
/// <typeparam name="T">Type of the returned value.</typeparam>
public class Result<T> : Result
{
    public T? Value { get; }

    protected Result(bool isSuccess, string message, T? value = default, IReadOnlyList<string>? errors = null)
        : base(isSuccess, message, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value, string message = "Operation succeeded.") => new(true, message, value);
    public new static Result<T> Failure(string message, IReadOnlyList<string>? errors = null) => new(false, message, default, errors);
    public new static Result<T> Failure(string message, string error) => new(false, message, default, new[] { error });
}
