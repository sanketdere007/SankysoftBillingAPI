using System.Data;
using Billing_Software_Api.Data;
using Billing_Software_Api.Helpers;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

/// <summary>
/// ADO.NET repository implementation for authentication and credential verification.
/// </summary>
public class AuthRepository : IAuthRepository
{
    private readonly DbHelper _dbHelper;
    private readonly IJwtHelper _jwtHelper;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(DbHelper dbHelper, IJwtHelper jwtHelper, ILogger<AuthRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _jwtHelper = jwtHelper ?? throw new ArgumentNullException(nameof(jwtHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticates an employee by verifying their username and password hash, then generating a JWT token.
    /// </summary>
    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest loginRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(loginRequest.Emp_UserName) || string.IsNullOrWhiteSpace(loginRequest.Password))
            {
                return ApiResponse<LoginResponse>.FailureResult("Username and password are required.");
            }

            var parameters = new[]
            {
                DbHelper.CreateParameter("@Emp_UserName", loginRequest.Emp_UserName.Trim(), SqlDbType.NVarChar, 100),
                DbHelper.CreateParameter("@Emp_Password", loginRequest.Password, SqlDbType.VarChar, 255)
            };

            var (status, message, employee) = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "SP_Employee_Login",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        var statusVal = 0;
                        var messageVal = "Invalid Username or Password.";

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (colName.Equals("Status", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                statusVal = Convert.ToInt32(reader.GetValue(i));
                            }
                            else if (colName.Equals("Message", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                messageVal = Convert.ToString(reader.GetValue(i)) ?? messageVal;
                            }
                        }

                        if (statusVal == 1)
                        {
                            var emp = EmployeeRepository.MapEmployeeFromReader(reader, includePasswordHash: false);
                            return (Status: 1, Message: messageVal, Employee: emp);
                        }

                        return (Status: statusVal, Message: messageVal, Employee: (EmployeeModel?)null);
                    }

                    return (Status: 0, Message: "Invalid Username or Password.", Employee: (EmployeeModel?)null);
                },
                cancellationToken: cancellationToken);

            if (status != 1 || employee == null)
            {
                _logger.LogWarning("Failed login attempt for username: {UserName}. DB message: {Message}", loginRequest.Emp_UserName, message);
                return ApiResponse<LoginResponse>.FailureResult(message);
            }

            // Generate JWT Token
            var (token, expiration) = _jwtHelper.GenerateToken(employee);

            var loginResponse = new LoginResponse
            {
                Token = token,
                Expiration = expiration,
                Emp_Id = employee.Emp_Id,
                Emp_FirstName = employee.Emp_FirstName,
                Emp_MiddleName = employee.Emp_MiddleName,
                Emp_LastName = employee.Emp_LastName,
                Emp_Email = employee.Emp_Email,
                Emp_MobileNumber = employee.Emp_MobileNumber,
                Emp_UserName = employee.Emp_UserName,
                Emp_Gender = employee.Emp_Gender,
                Emp_Role = employee.Emp_Role,
                Emp_BranchId = employee.Emp_BranchId,
                Emp_CompId = employee.Emp_CompId,
                Emp_Department = employee.Emp_Department,
                Emp_Designation = employee.Emp_Designation,
                Emp_JoiningDate = employee.Emp_JoiningDate,
                Emp_IsActive = employee.Emp_IsActive
            };

            _logger.LogInformation("Employee successfully authenticated. Emp_Id: {EmpId}, UserName: {UserName}", employee.Emp_Id, employee.Emp_UserName);
            return ApiResponse<LoginResponse>.SuccessResult(loginResponse, message);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred during login for user: {UserName}", loginRequest.Emp_UserName);
            return ApiResponse<LoginResponse>.FailureResult(
                message: "Authentication service is currently unavailable.",
                error: $"Database connectivity error: {sqlEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during login for user: {UserName}", loginRequest.Emp_UserName);
            return ApiResponse<LoginResponse>.FailureResult(
                message: "An unexpected error occurred during authentication.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Authenticates and fetches an employee record from SQL Server using SP_Employee_Login.
    /// </summary>
    public async Task<EmployeeModel?> GetEmployeeByLoginCredentialsAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            DbHelper.CreateParameter("@Emp_UserName", username, SqlDbType.NVarChar, 100),
            DbHelper.CreateParameter("@Emp_Password", password, SqlDbType.VarChar, 255)
        };

        return await _dbHelper.ExecuteStoredProcedureAsync(
            procedureName: "SP_Employee_Login",
            parameters: parameters,
            mapReaderFunc: async reader =>
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    return EmployeeRepository.MapEmployeeFromReader(reader, includePasswordHash: false);
                }
                return null;
            },
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Fetches an employee record by username from SQL Server using ADO.NET and Stored Procedure.
    /// </summary>
    public async Task<EmployeeModel?> GetEmployeeByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var parameters = new[]
        {
            DbHelper.CreateParameter("@Emp_UserName", username, SqlDbType.NVarChar, 100)
        };

        return await _dbHelper.ExecuteStoredProcedureAsync(
            procedureName: "SP_Employee_GetByUserName",
            parameters: parameters,
            mapReaderFunc: async reader =>
            {
                if (await reader.ReadAsync(cancellationToken))
                {
                    return EmployeeRepository.MapEmployeeFromReader(reader, includePasswordHash: true);
                }
                return null;
            },
            cancellationToken: cancellationToken);
    }
}
