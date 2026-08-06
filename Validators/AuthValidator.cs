using Billing_Software_Api.Common;
using Billing_Software_Api.DTOs.Auth;

namespace Billing_Software_Api.Validators;

/// <summary>
/// Validator placeholder template for Auth module request DTOs.
/// </summary>
public class AuthValidator : BaseValidator<LoginRequestDto>
{
    public override Task<Result> ValidateAsync(LoginRequestDto instance, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
