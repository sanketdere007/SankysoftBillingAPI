using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repository.Interfaces;

/// <summary>
/// Repository interface defining data access operations for Customer management using ADO.NET and Stored Procedures.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Executes SP_Customer_InsertOrUpdate stored procedure passing JSON serialized customer data.
    /// Inserts new customer if Cust_Id is 0, or updates existing customer if Cust_Id > 0.
    /// </summary>
    Task<ApiResponse<CustomerSaveResult>> SaveCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches customer records from SQL Server using SP_Customer_GetAll stored procedure with optional filters.
    /// </summary>
    Task<ApiResponse<List<CustomerListModel>>> GetAllCustomersAsync(CustomerFilterDto? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches complete customer details by ID from SQL Server using ADO.NET / Stored Procedure.
    /// </summary>
    Task<ApiResponse<CustomerListModel>> GetCustomerByIdAsync(int custId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches customer-wise outstanding (pending balance) using SP_Customer_Outstanding_GetAll.
    /// </summary>
    Task<ApiResponse<PagedListResult<CustomerOutstandingModel>>> GetCustomerOutstandingAsync(CustomerOutstandingFilterDto? filter = null, CancellationToken cancellationToken = default);
}
