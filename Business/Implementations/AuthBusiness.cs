using Billing_Software_Api.Business.Interfaces;
using Billing_Software_Api.Common;
using Billing_Software_Api.DTOs.Auth;
using Billing_Software_Api.Repositories.Interfaces;

namespace Billing_Software_Api.Business.Implementations;

/// <summary>
/// Business layer implementation for authentication operations coordinating between controllers and repositories.
/// </summary>
public class AuthBusiness : IAuthBusiness
{
    private readonly IAuthRepository _authRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthBusiness(IAuthRepository authRepository, IUnitOfWork unitOfWork)
    {
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
