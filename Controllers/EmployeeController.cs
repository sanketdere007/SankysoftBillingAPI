using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Employee Controller managing employee registration, updates, and data retrieval.
/// Protected by JWT Authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(IEmployeeRepository employeeRepository, ILogger<EmployeeController> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// API 1: Insert or Update Employee (using SP_Employee_InsertOrUpdate)
    /// Converts Employee model to JSON and passes it to @EmpJsonData parameter.
    /// If Emp_Id = 0 -> Performs Insert.
    /// If Emp_Id > 0 -> Performs Update.
    /// </summary>
    /// <param name="employee">Employee data payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status, Message, and Emp_Id</returns>
    [HttpPost("InsertorUpdateEmployee")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertorUpdateEmployee([FromBody] EmployeeModel employee, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<EmployeeSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new EmployeeSaveResult { Status = false, Message = "Validation failed.", Emp_Id = employee.Emp_Id }));
        }

        // Additional validation: If inserting a new employee (Emp_Id = 0), password is required
        if (employee.Emp_Id == 0 && string.IsNullOrWhiteSpace(employee.Emp_Password))
        {
            return BadRequest(ApiResponse<EmployeeSaveResult>.FailureResult(
                message: "Password is required when creating a new employee.",
                error: "Emp_Password field cannot be empty for new employee.",
                data: new EmployeeSaveResult { Status = false, Message = "Password is required.", Emp_Id = 0 }));
        }

        var result = await _employeeRepository.SaveEmployeeAsync(employee, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// API 3: Get All Employees
    /// Fetches and returns all active employee records from SQL Server via ADO.NET.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of employees</returns>
    [HttpGet("GetAllEmployees")]
    [ProducesResponseType(typeof(ApiResponse<List<EmployeeModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<EmployeeModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllEmployees(CancellationToken cancellationToken = default)
    {
        var result = await _employeeRepository.GetAllEmployeesAsync(cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// API 4: Get Employee By Id
    /// Returns complete employee details for the specified employee ID.
    /// </summary>
    /// <param name="Emp_Id">Employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee details</returns>
    [HttpGet("GetEmployeeById/{Emp_Id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmployeeById(int Emp_Id, CancellationToken cancellationToken = default)
    {
        if (Emp_Id <= 0)
        {
            return BadRequest(ApiResponse<EmployeeModel>.FailureResult("Employee ID must be a positive integer."));
        }

        var result = await _employeeRepository.GetEmployeeByIdAsync(Emp_Id, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return NotFound(result);
    }
}
