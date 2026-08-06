using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Helpers;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

/// <summary>
/// ADO.NET repository implementation for Employee data operations using Stored Procedures.
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly DbHelper _dbHelper;
    private readonly IJwtHelper _jwtHelper;
    private readonly ILogger<EmployeeRepository> _logger;

    public EmployeeRepository(DbHelper dbHelper, IJwtHelper jwtHelper, ILogger<EmployeeRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _jwtHelper = jwtHelper ?? throw new ArgumentNullException(nameof(jwtHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes SP_Employee_InsertOrUpdate by serializing the employee model to JSON and passing as @EmpJsonData.
    /// Returns Status, Message, and Emp_Id.
    /// </summary>
    public async Task<ApiResponse<EmployeeSaveResult>> SaveEmployeeAsync(EmployeeModel employee, CancellationToken cancellationToken = default)
    {
        try
        {
            // If plaintext password is provided during insert or password update, hash it before saving to DB
            if (!string.IsNullOrWhiteSpace(employee.Emp_Password))
            {
                employee.Emp_PasswordHash = _jwtHelper.HashPassword(employee.Emp_Password);
            }

            // Convert employee model into JSON string to pass into @EmpJsonData parameter
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null // Keep exact property names for SQL OPENJSON mapping
            };
            var jsonData = JsonSerializer.Serialize(employee, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "SP_Employee_InsertOrUpdate",
                jsonParameterName: "@EmpJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new EmployeeSaveResult();
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        // Stored procedure returns Status, Message, Emp_Id
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var colName = reader.GetName(i);
                            if (colName.Equals("Status", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                var statusVal = reader.GetValue(i);
                                result.Status = statusVal is bool b ? b : Convert.ToInt32(statusVal) == 1;
                            }
                            else if (colName.Equals("Message", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Message = Convert.ToString(reader.GetValue(i)) ?? string.Empty;
                            }
                            else if (colName.Equals("Emp_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Emp_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<EmployeeSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<EmployeeSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save employee record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving employee record. Emp_Id: {EmpId}", employee.Emp_Id);
            return ApiResponse<EmployeeSaveResult>.FailureResult(
                message: "A database error occurred while processing employee data.",
                error: "Database constraint or connection failure.",
                data: new EmployeeSaveResult { Status = false, Message = "Database error occurred.", Emp_Id = employee.Emp_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving employee record. Emp_Id: {EmpId}", employee.Emp_Id);
            return ApiResponse<EmployeeSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving employee.",
                error: ex.Message,
                data: new EmployeeSaveResult { Status = false, Message = "Unexpected error occurred.", Emp_Id = employee.Emp_Id });
        }
    }

    /// <summary>
    /// Fetches all active employee records from SQL Server using ADO.NET.
    /// </summary>
    public async Task<ApiResponse<List<EmployeeModel>>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var employees = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "SP_Employee_GetAll",
                parameters: null,
                mapReaderFunc: async reader =>
                {
                    var list = new List<EmployeeModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapEmployeeFromReader(reader, includePasswordHash: false));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<EmployeeModel>>.SuccessResult(
                data: employees,
                message: $"Successfully retrieved {employees.Count} employee record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching all employees.");
            return ApiResponse<List<EmployeeModel>>.FailureResult(
                message: "Unable to retrieve employee list from database.",
                error: "Database execution failure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching all employees.");
            return ApiResponse<List<EmployeeModel>>.FailureResult(
                message: "An unexpected error occurred while fetching employees.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Fetches complete employee details by ID from SQL Server using ADO.NET.
    /// </summary>
    public async Task<ApiResponse<EmployeeModel>> GetEmployeeByIdAsync(int empId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (empId <= 0)
            {
                return ApiResponse<EmployeeModel>.FailureResult("Invalid Employee ID specified.");
            }

            var parameters = new[]
            {
                DbHelper.CreateParameter("@Emp_Id", empId, SqlDbType.Int)
            };

            var employee = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "SP_Employee_GetById",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        return MapEmployeeFromReader(reader, includePasswordHash: false);
                    }
                    return null;
                },
                cancellationToken: cancellationToken);

            if (employee == null)
            {
                return ApiResponse<EmployeeModel>.FailureResult(
                    message: $"Employee with ID {empId} was not found.");
            }

            return ApiResponse<EmployeeModel>.SuccessResult(
                data: employee,
                message: "Employee details retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching employee with ID: {EmpId}", empId);
            return ApiResponse<EmployeeModel>.FailureResult(
                message: "A database error occurred while retrieving employee details.",
                error: "Database query failure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching employee with ID: {EmpId}", empId);
            return ApiResponse<EmployeeModel>.FailureResult(
                message: "An unexpected error occurred while retrieving employee details.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Helper method to safely map SqlDataReader columns to an EmployeeModel instance.
    /// </summary>
    public static EmployeeModel MapEmployeeFromReader(SqlDataReader reader, bool includePasswordHash = false)
    {
        var model = new EmployeeModel();

        if (HasColumn(reader, "Emp_Id") && !reader.IsDBNull(reader.GetOrdinal("Emp_Id")))
            model.Emp_Id = Convert.ToInt32(reader["Emp_Id"]);

        if (HasColumn(reader, "Emp_FirstName") && !reader.IsDBNull(reader.GetOrdinal("Emp_FirstName")))
            model.Emp_FirstName = Convert.ToString(reader["Emp_FirstName"]) ?? string.Empty;

        if (HasColumn(reader, "Emp_MiddleName") && !reader.IsDBNull(reader.GetOrdinal("Emp_MiddleName")))
            model.Emp_MiddleName = Convert.ToString(reader["Emp_MiddleName"]);

        if (HasColumn(reader, "Emp_LastName") && !reader.IsDBNull(reader.GetOrdinal("Emp_LastName")))
            model.Emp_LastName = Convert.ToString(reader["Emp_LastName"]);

        if (HasColumn(reader, "Emp_Email") && !reader.IsDBNull(reader.GetOrdinal("Emp_Email")))
            model.Emp_Email = Convert.ToString(reader["Emp_Email"]);

        if (HasColumn(reader, "Emp_MobileNumber") && !reader.IsDBNull(reader.GetOrdinal("Emp_MobileNumber")))
            model.Emp_MobileNumber = Convert.ToString(reader["Emp_MobileNumber"]) ?? string.Empty;

        if (HasColumn(reader, "Emp_Gender") && !reader.IsDBNull(reader.GetOrdinal("Emp_Gender")))
            model.Emp_Gender = Convert.ToString(reader["Emp_Gender"]);

        if (HasColumn(reader, "Emp_UserName") && !reader.IsDBNull(reader.GetOrdinal("Emp_UserName")))
            model.Emp_UserName = Convert.ToString(reader["Emp_UserName"]) ?? string.Empty;

        if (includePasswordHash && HasColumn(reader, "Emp_PasswordHash") && !reader.IsDBNull(reader.GetOrdinal("Emp_PasswordHash")))
            model.Emp_PasswordHash = Convert.ToString(reader["Emp_PasswordHash"]);

        if (HasColumn(reader, "Emp_Role") && !reader.IsDBNull(reader.GetOrdinal("Emp_Role")))
            model.Emp_Role = Convert.ToString(reader["Emp_Role"]) ?? "Employee";

        if (HasColumn(reader, "Emp_BranchId") && !reader.IsDBNull(reader.GetOrdinal("Emp_BranchId")))
            model.Emp_BranchId = Convert.ToInt32(reader["Emp_BranchId"]);

        if (HasColumn(reader, "Emp_CompId") && !reader.IsDBNull(reader.GetOrdinal("Emp_CompId")))
            model.Emp_CompId = Convert.ToInt32(reader["Emp_CompId"]);

        if (HasColumn(reader, "Emp_Department") && !reader.IsDBNull(reader.GetOrdinal("Emp_Department")))
            model.Emp_Department = Convert.ToString(reader["Emp_Department"]);

        if (HasColumn(reader, "Emp_Designation") && !reader.IsDBNull(reader.GetOrdinal("Emp_Designation")))
            model.Emp_Designation = Convert.ToString(reader["Emp_Designation"]);

        if (HasColumn(reader, "Emp_Salary") && !reader.IsDBNull(reader.GetOrdinal("Emp_Salary")))
            model.Emp_Salary = Convert.ToDecimal(reader["Emp_Salary"]);

        if (HasColumn(reader, "Emp_Address") && !reader.IsDBNull(reader.GetOrdinal("Emp_Address")))
            model.Emp_Address = Convert.ToString(reader["Emp_Address"]);

        if (HasColumn(reader, "Emp_City") && !reader.IsDBNull(reader.GetOrdinal("Emp_City")))
            model.Emp_City = Convert.ToString(reader["Emp_City"]);

        if (HasColumn(reader, "Emp_State") && !reader.IsDBNull(reader.GetOrdinal("Emp_State")))
            model.Emp_State = Convert.ToString(reader["Emp_State"]);

        if (HasColumn(reader, "Emp_Pincode") && !reader.IsDBNull(reader.GetOrdinal("Emp_Pincode")))
            model.Emp_Pincode = Convert.ToString(reader["Emp_Pincode"]);

        if (HasColumn(reader, "Emp_DateOfBirth") && !reader.IsDBNull(reader.GetOrdinal("Emp_DateOfBirth")))
            model.Emp_DateOfBirth = Convert.ToDateTime(reader["Emp_DateOfBirth"]);

        if (HasColumn(reader, "Emp_JoiningDate") && !reader.IsDBNull(reader.GetOrdinal("Emp_JoiningDate")))
            model.Emp_DateOfJoining = Convert.ToDateTime(reader["Emp_JoiningDate"]);
        else if (HasColumn(reader, "Emp_DateOfJoining") && !reader.IsDBNull(reader.GetOrdinal("Emp_DateOfJoining")))
            model.Emp_DateOfJoining = Convert.ToDateTime(reader["Emp_DateOfJoining"]);

        if (HasColumn(reader, "Emp_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Emp_IsActive")))
            model.IsActive = Convert.ToBoolean(reader["Emp_IsActive"]);
        else if (HasColumn(reader, "IsActive") && !reader.IsDBNull(reader.GetOrdinal("IsActive")))
            model.IsActive = Convert.ToBoolean(reader["IsActive"]);

        if (HasColumn(reader, "CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("CreatedDate")))
            model.CreatedDate = Convert.ToDateTime(reader["CreatedDate"]);

        if (HasColumn(reader, "ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("ModifiedDate")))
            model.ModifiedDate = Convert.ToDateTime(reader["ModifiedDate"]);

        return model;
    }

    private static bool HasColumn(SqlDataReader reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
