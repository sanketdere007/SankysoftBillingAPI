using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

/// <summary>
/// ADO.NET repository implementation for Customer data operations using Stored Procedures.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(DbHelper dbHelper, ILogger<CustomerRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes SP_Customer_InsertOrUpdate by serializing the Customer model to JSON and passing as @CustJsonData.
    /// Returns Status, Message, Cust_Id, and Cust_Code.
    /// </summary>
    public async Task<ApiResponse<CustomerSaveResult>> SaveCustomerAsync(CustomerModel customer, CancellationToken cancellationToken = default)
    {
        try
        {
            // Convert customer model into JSON string to pass into @CustJsonData parameter
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null // Preserve PascalCase property names matching SQL JSON_VALUE
            };
            var jsonData = JsonSerializer.Serialize(customer, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_Customer_InsertOrUpdate",
                jsonParameterName: "@CustJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new CustomerSaveResult();
                    if (await reader.ReadAsync(cancellationToken))
                    {
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
                            else if (colName.Equals("Cust_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Cust_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                            else if (colName.Equals("Cust_Code", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Cust_Code = Convert.ToString(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<CustomerSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<CustomerSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save customer record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving customer record. Cust_Id: {CustId}", customer.Cust_Id);
            return ApiResponse<CustomerSaveResult>.FailureResult(
                message: sqlEx.Message.Contains("Mobile Number already exists") 
                    ? "Mobile Number already exists." 
                    : "A database error occurred while processing customer data.",
                error: sqlEx.Message,
                data: new CustomerSaveResult { Status = false, Message = sqlEx.Message, Cust_Id = customer.Cust_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving customer record. Cust_Id: {CustId}", customer.Cust_Id);
            return ApiResponse<CustomerSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving customer.",
                error: ex.Message,
                data: new CustomerSaveResult { Status = false, Message = "Unexpected error occurred.", Cust_Id = customer.Cust_Id });
        }
    }

    /// <summary>
    /// Fetches customer records from SQL Server using SP_Customer_GetAll stored procedure with optional filters.
    /// </summary>
    public async Task<ApiResponse<List<CustomerListModel>>> GetAllCustomersAsync(CustomerFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Search", string.IsNullOrWhiteSpace(filter?.Search) ? (object)string.Empty : filter.Search.Trim(), SqlDbType.NVarChar, 100),
                DbHelper.CreateParameter("@AreaId", string.IsNullOrWhiteSpace(filter?.AreaId) ? (object)"0" : filter.AreaId.Trim(), SqlDbType.NVarChar, 250),
                DbHelper.CreateParameter("@CityId", string.IsNullOrWhiteSpace(filter?.CityId) ? (object)"0" : filter.CityId.Trim(), SqlDbType.NVarChar, 100),
                DbHelper.CreateParameter("@StateId", string.IsNullOrWhiteSpace(filter?.StateId) ? (object)"0" : filter.StateId.Trim(), SqlDbType.NVarChar, 100),
                DbHelper.CreateParameter("@BranchId", filter?.BranchId ?? 0, SqlDbType.Int),
                DbHelper.CreateParameter("@CompId", filter?.CompId ?? 0, SqlDbType.Int),
                DbHelper.CreateParameter("@IsActive", filter?.IsActive.HasValue == true ? (object)filter.IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var customers = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Customer_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<CustomerListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapCustomerFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<CustomerListModel>>.SuccessResult(
                data: customers,
                message: $"Successfully retrieved {customers.Count} customer record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching customers using SP_Customer_GetAll.");
            return ApiResponse<List<CustomerListModel>>.FailureResult(
                message: "Unable to retrieve customer list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching customers.");
            return ApiResponse<List<CustomerListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching customers.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Fetches complete customer details by ID from SQL Server using SP_Customer_GetById stored procedure.
    /// </summary>
    public async Task<ApiResponse<CustomerListModel>> GetCustomerByIdAsync(int custId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (custId <= 0)
            {
                return ApiResponse<CustomerListModel>.FailureResult("Invalid Customer ID specified.");
            }

            var parameters = new[]
            {
                DbHelper.CreateParameter("@Cust_Id", custId, SqlDbType.Int)
            };

            var customer = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Customer_GetById",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        return MapCustomerFromReader(reader);
                    }
                    return null;
                },
                cancellationToken: cancellationToken);

            if (customer == null)
            {
                return ApiResponse<CustomerListModel>.FailureResult(
                    message: $"Customer with ID {custId} was not found.");
            }

            return ApiResponse<CustomerListModel>.SuccessResult(
                data: customer,
                message: "Customer details retrieved successfully.");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching customer with ID: {CustId}", custId);
            return ApiResponse<CustomerListModel>.FailureResult(
                message: "A database error occurred while retrieving customer details.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching customer with ID: {CustId}", custId);
            return ApiResponse<CustomerListModel>.FailureResult(
                message: "An unexpected error occurred while retrieving customer details.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Helper method to safely map SqlDataReader columns to a CustomerListModel instance.
    /// </summary>
    public static CustomerListModel MapCustomerFromReader(SqlDataReader reader)
    {
        var model = new CustomerListModel();

        if (HasColumn(reader, "Cust_Id") && !reader.IsDBNull(reader.GetOrdinal("Cust_Id")))
            model.Cust_Id = Convert.ToInt32(reader["Cust_Id"]);

        if (HasColumn(reader, "Cust_Code") && !reader.IsDBNull(reader.GetOrdinal("Cust_Code")))
            model.Cust_Code = Convert.ToString(reader["Cust_Code"]);

        if (HasColumn(reader, "Cust_Name") && !reader.IsDBNull(reader.GetOrdinal("Cust_Name")))
            model.Cust_Name = Convert.ToString(reader["Cust_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Cust_CompanyName") && !reader.IsDBNull(reader.GetOrdinal("Cust_CompanyName")))
            model.Cust_CompanyName = Convert.ToString(reader["Cust_CompanyName"]);

        if (HasColumn(reader, "Cust_MobileNo") && !reader.IsDBNull(reader.GetOrdinal("Cust_MobileNo")))
            model.Cust_MobileNo = Convert.ToString(reader["Cust_MobileNo"]) ?? string.Empty;

        if (HasColumn(reader, "Cust_AlternateMobileNo") && !reader.IsDBNull(reader.GetOrdinal("Cust_AlternateMobileNo")))
            model.Cust_AlternateMobileNo = Convert.ToString(reader["Cust_AlternateMobileNo"]);

        if (HasColumn(reader, "Cust_Email") && !reader.IsDBNull(reader.GetOrdinal("Cust_Email")))
            model.Cust_Email = Convert.ToString(reader["Cust_Email"]);

        if (HasColumn(reader, "Cust_GSTNo") && !reader.IsDBNull(reader.GetOrdinal("Cust_GSTNo")))
            model.Cust_GSTNo = Convert.ToString(reader["Cust_GSTNo"]);

        if (HasColumn(reader, "Cust_PANNo") && !reader.IsDBNull(reader.GetOrdinal("Cust_PANNo")))
            model.Cust_PANNo = Convert.ToString(reader["Cust_PANNo"]);

        if (HasColumn(reader, "Cust_Address") && !reader.IsDBNull(reader.GetOrdinal("Cust_Address")))
            model.Cust_Address = Convert.ToString(reader["Cust_Address"]);

        if (HasColumn(reader, "Cust_AreaId") && !reader.IsDBNull(reader.GetOrdinal("Cust_AreaId")))
            model.Cust_AreaId = Convert.ToInt32(reader["Cust_AreaId"]);

        if (HasColumn(reader, "Cust_Area") && !reader.IsDBNull(reader.GetOrdinal("Cust_Area")))
            model.Cust_Area = Convert.ToString(reader["Cust_Area"]);

        if (HasColumn(reader, "Cust_CityId") && !reader.IsDBNull(reader.GetOrdinal("Cust_CityId")))
            model.Cust_CityId = Convert.ToInt32(reader["Cust_CityId"]);

        if (HasColumn(reader, "Cust_City") && !reader.IsDBNull(reader.GetOrdinal("Cust_City")))
            model.Cust_City = Convert.ToString(reader["Cust_City"]);

        if (HasColumn(reader, "Cust_StateId") && !reader.IsDBNull(reader.GetOrdinal("Cust_StateId")))
            model.Cust_StateId = Convert.ToInt32(reader["Cust_StateId"]);

        if (HasColumn(reader, "Cust_State") && !reader.IsDBNull(reader.GetOrdinal("Cust_State")))
            model.Cust_State = Convert.ToString(reader["Cust_State"]);

        if (HasColumn(reader, "Cust_Pincode") && !reader.IsDBNull(reader.GetOrdinal("Cust_Pincode")))
            model.Cust_Pincode = Convert.ToString(reader["Cust_Pincode"]);

        if (HasColumn(reader, "Cust_Country") && !reader.IsDBNull(reader.GetOrdinal("Cust_Country")))
            model.Cust_Country = Convert.ToString(reader["Cust_Country"]) ?? "India";

        if (HasColumn(reader, "Cust_BranchId") && !reader.IsDBNull(reader.GetOrdinal("Cust_BranchId")))
            model.Cust_BranchId = Convert.ToInt32(reader["Cust_BranchId"]);

        if (HasColumn(reader, "Cust_CompId") && !reader.IsDBNull(reader.GetOrdinal("Cust_CompId")))
            model.Cust_CompId = Convert.ToInt32(reader["Cust_CompId"]);

        if (HasColumn(reader, "Cust_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Cust_IsActive")))
            model.Cust_IsActive = Convert.ToBoolean(reader["Cust_IsActive"]);

        if (HasColumn(reader, "Cust_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("Cust_CreatedBy")))
            model.Cust_CreatedBy = Convert.ToInt32(reader["Cust_CreatedBy"]);

        if (HasColumn(reader, "Cust_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Cust_CreatedDate")))
            model.Cust_CreatedDate = Convert.ToDateTime(reader["Cust_CreatedDate"]);

        if (HasColumn(reader, "Cust_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("Cust_ModifiedBy")))
            model.Cust_ModifiedBy = Convert.ToInt32(reader["Cust_ModifiedBy"]);

        if (HasColumn(reader, "Cust_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Cust_ModifiedDate")))
            model.Cust_ModifiedDate = Convert.ToDateTime(reader["Cust_ModifiedDate"]);

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
