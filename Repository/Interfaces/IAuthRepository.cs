using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

/// <summary>
/// Repository interface defining authentication operations using ADO.NET and JWT generation.
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Authenticates employee credentials, compares password hash, and returns JWT token upon success.
    /// </summary>
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches an employee record using SP_Employee_Login from SQL Server.
    /// </summary>
    Task<EmployeeModel?> GetEmployeeByLoginCredentialsAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches an employee record by username from SQL Server using ADO.NET.
    /// </summary>
    Task<EmployeeModel?> GetEmployeeByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
