using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

/// <summary>
/// Repository interface defining data access operations for Employee management using ADO.NET and Stored Procedures.
/// </summary>
public interface IEmployeeRepository
{
    /// <summary>
    /// Executes SP_Employee_InsertOrUpdate stored procedure passing JSON serialized employee data.
    /// Inserts new employee if Emp_Id is 0, or updates existing employee if Emp_Id > 0.
    /// </summary>
    Task<ApiResponse<EmployeeSaveResult>> SaveEmployeeAsync(EmployeeModel employee, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches all active employee records from SQL Server using ADO.NET.
    /// </summary>
    Task<ApiResponse<List<EmployeeModel>>> GetAllEmployeesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches complete employee details by ID from SQL Server using ADO.NET.
    /// </summary>
    Task<ApiResponse<EmployeeModel>> GetEmployeeByIdAsync(int empId, CancellationToken cancellationToken = default);
}
