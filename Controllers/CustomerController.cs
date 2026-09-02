using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billing_Software_Api.Controllers;

/// <summary>
/// Customer Controller managing customer registration, updates, and data retrieval.
/// Protected by JWT Authentication.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomerController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerController> _logger;

    public CustomerController(ICustomerRepository customerRepository, ILogger<CustomerController> logger)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Insert or Update Customer (using SP_Customer_InsertOrUpdate)
    /// Converts Customer model to JSON and passes it to the @CustJsonData stored procedure parameter.
    /// If Cust_Id = 0 -> Performs Insert and auto-generates Cust_Code (e.g. CUST000001).
    /// If Cust_Id > 0 -> Performs Update on the existing customer record.
    /// </summary>
    /// <param name="customer">Customer data payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Status, Message, Cust_Id, and Cust_Code</returns>
    [HttpPost("InsertorUpdateCustomer")]
    [ProducesResponseType(typeof(ApiResponse<CustomerSaveResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerSaveResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> InsertorUpdateCustomer([FromBody] CustomerModel customer, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<CustomerSaveResult>.FailureResult(
                message: "Validation failed.",
                error: errors,
                data: new CustomerSaveResult { Status = false, Message = "Validation failed.", Cust_Id = customer.Cust_Id }));
        }

        var result = await _customerRepository.SaveCustomerAsync(customer, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Get All Customers (using SP_Customer_GetAll)
    /// Fetches and returns customer records from SQL Server via SP_Customer_GetAll with optional search and filters.
    /// </summary>
    /// <param name="filter">Optional query filter parameters: Search, AreaId, CityId, StateId, BranchId, CompId, IsActive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customers matching filter criteria</returns>
    [HttpGet("GetAllCustomers")]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerListModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<CustomerListModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllCustomers([FromQuery] CustomerFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        var result = await _customerRepository.GetAllCustomersAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    /// <summary>
    /// Get Customer By Id
    /// Returns complete customer details for the specified customer ID.
    /// </summary>
    /// <param name="Cust_Id">Customer ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Customer details</returns>
    [HttpGet("GetCustomerById/{Cust_Id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerListModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CustomerListModel>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<CustomerListModel>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCustomerById(int Cust_Id, CancellationToken cancellationToken = default)
    {
        if (Cust_Id <= 0)
        {
            return BadRequest(ApiResponse<CustomerListModel>.FailureResult("Customer ID must be a positive integer."));
        }

        var result = await _customerRepository.GetCustomerByIdAsync(Cust_Id, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return NotFound(result);
    }

    /// <summary>
    /// Get customer-wise outstanding (pending balance) using SP_Customer_Outstanding_GetAll.
    /// </summary>
    [HttpGet("GetAllCustomerOutstanding")]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<CustomerOutstandingModel>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedListResult<CustomerOutstandingModel>>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllCustomerOutstanding(
        [FromQuery] CustomerOutstandingFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _customerRepository.GetCustomerOutstandingAsync(filter, cancellationToken);

        if (result.Status)
        {
            return Ok(result);
        }

        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
