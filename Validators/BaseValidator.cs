using Billing_Software_Api.Common;

namespace Billing_Software_Api.Validators;

/// <summary>
/// Generic validator interface contract for asynchronous DTO/input validation.
/// </summary>
/// <typeparam name="T">Type of object to validate.</typeparam>
public interface IValidator<in T>
{
    Task<Result> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}

/// <summary>
/// Abstract base validator providing helper validation routines.
/// </summary>
/// <typeparam name="T">Type of object being validated.</typeparam>
public abstract class BaseValidator<T> : IValidator<T>
{
    public abstract Task<Result> ValidateAsync(T instance, CancellationToken cancellationToken = default);

    protected static Result Success() => Result.Success();

    protected static Result Failure(string message, IReadOnlyList<string>? errors = null) =>
        Result.Failure(message, errors);

    protected static Result Failure(string message, string error) =>
        Result.Failure(message, error);
}
