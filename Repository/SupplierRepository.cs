using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

public class SupplierRepository : ISupplierRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<SupplierRepository> _logger;

    public SupplierRepository(DbHelper dbHelper, ILogger<SupplierRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponse<SupplierSaveResult>> SaveSupplierAsync(SupplierModel supplier, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = null 
            };
            var jsonData = JsonSerializer.Serialize(supplier, jsonOptions);

            var saveResult = await _dbHelper.ExecuteStoredProcedureWithJsonAsync(
                procedureName: "dbo.SP_Supplier_InsertOrUpdate",
                jsonParameterName: "@SuppJsonData",
                jsonData: jsonData,
                mapReaderFunc: async reader =>
                {
                    var result = new SupplierSaveResult();
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
                            else if (colName.Equals("Supp_Id", StringComparison.OrdinalIgnoreCase) && !reader.IsDBNull(i))
                            {
                                result.Supp_Id = Convert.ToInt32(reader.GetValue(i));
                            }
                        }
                    }
                    return result;
                },
                cancellationToken: cancellationToken);

            if (saveResult.Status)
            {
                return ApiResponse<SupplierSaveResult>.SuccessResult(saveResult, saveResult.Message);
            }

            return ApiResponse<SupplierSaveResult>.FailureResult(
                message: string.IsNullOrWhiteSpace(saveResult.Message) ? "Failed to save supplier record." : saveResult.Message,
                error: null,
                data: saveResult);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while saving supplier record. Supp_Id: {SuppId}", supplier.Supp_Id);
            return ApiResponse<SupplierSaveResult>.FailureResult(
                message: sqlEx.Message.Contains("already exists") 
                    ? "Supplier code or mobile already exists." 
                    : "A database error occurred while processing supplier data.",
                error: sqlEx.Message,
                data: new SupplierSaveResult { Status = false, Message = sqlEx.Message, Supp_Id = supplier.Supp_Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while saving supplier record. Supp_Id: {SuppId}", supplier.Supp_Id);
            return ApiResponse<SupplierSaveResult>.FailureResult(
                message: "An unexpected error occurred while saving supplier.",
                error: ex.Message,
                data: new SupplierSaveResult { Status = false, Message = "Unexpected error occurred.", Supp_Id = supplier.Supp_Id });
        }
    }

    public async Task<ApiResponse<List<SupplierListModel>>> GetAllSuppliersAsync(SupplierFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Supp_Id", filter?.Supp_Id ?? (object)DBNull.Value, SqlDbType.Int),
                DbHelper.CreateParameter("@Supp_IsActive", filter?.Supp_IsActive.HasValue == true ? (object)filter.Supp_IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var suppliers = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Supplier_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<SupplierListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapSupplierFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<SupplierListModel>>.SuccessResult(
                data: suppliers,
                message: $"Successfully retrieved {suppliers.Count} supplier record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching suppliers using SP_Supplier_GetAll.");
            return ApiResponse<List<SupplierListModel>>.FailureResult(
                message: "Unable to retrieve supplier list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching suppliers.");
            return ApiResponse<List<SupplierListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching suppliers.",
                error: ex.Message);
        }
    }

    public static SupplierListModel MapSupplierFromReader(SqlDataReader reader)
    {
        var model = new SupplierListModel();

        if (HasColumn(reader, "Supp_Id") && !reader.IsDBNull(reader.GetOrdinal("Supp_Id")))
            model.Supp_Id = Convert.ToInt32(reader["Supp_Id"]);

        if (HasColumn(reader, "Supp_Code") && !reader.IsDBNull(reader.GetOrdinal("Supp_Code")))
            model.Supp_Code = Convert.ToString(reader["Supp_Code"]);

        if (HasColumn(reader, "Supp_Name") && !reader.IsDBNull(reader.GetOrdinal("Supp_Name")))
            model.Supp_Name = Convert.ToString(reader["Supp_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Supp_CompanyName") && !reader.IsDBNull(reader.GetOrdinal("Supp_CompanyName")))
            model.Supp_CompanyName = Convert.ToString(reader["Supp_CompanyName"]);

        if (HasColumn(reader, "Supp_MobileNo") && !reader.IsDBNull(reader.GetOrdinal("Supp_MobileNo")))
            model.Supp_MobileNo = Convert.ToString(reader["Supp_MobileNo"]) ?? string.Empty;

        if (HasColumn(reader, "Supp_AlternateMobileNo") && !reader.IsDBNull(reader.GetOrdinal("Supp_AlternateMobileNo")))
            model.Supp_AlternateMobileNo = Convert.ToString(reader["Supp_AlternateMobileNo"]);

        if (HasColumn(reader, "Supp_Email") && !reader.IsDBNull(reader.GetOrdinal("Supp_Email")))
            model.Supp_Email = Convert.ToString(reader["Supp_Email"]);

        if (HasColumn(reader, "Supp_GSTNo") && !reader.IsDBNull(reader.GetOrdinal("Supp_GSTNo")))
            model.Supp_GSTNo = Convert.ToString(reader["Supp_GSTNo"]);

        if (HasColumn(reader, "Supp_PANNo") && !reader.IsDBNull(reader.GetOrdinal("Supp_PANNo")))
            model.Supp_PANNo = Convert.ToString(reader["Supp_PANNo"]);

        if (HasColumn(reader, "Supp_Address") && !reader.IsDBNull(reader.GetOrdinal("Supp_Address")))
            model.Supp_Address = Convert.ToString(reader["Supp_Address"]);

        if (HasColumn(reader, "Supp_AreaId") && !reader.IsDBNull(reader.GetOrdinal("Supp_AreaId")))
            model.Supp_AreaId = Convert.ToInt32(reader["Supp_AreaId"]);

        if (HasColumn(reader, "Supp_AreaName") && !reader.IsDBNull(reader.GetOrdinal("Supp_AreaName")))
            model.Supp_AreaName = Convert.ToString(reader["Supp_AreaName"]);
        else if (HasColumn(reader, "Area_Name") && !reader.IsDBNull(reader.GetOrdinal("Area_Name")))
            model.Supp_AreaName = Convert.ToString(reader["Area_Name"]);

        if (HasColumn(reader, "Supp_CityId") && !reader.IsDBNull(reader.GetOrdinal("Supp_CityId")))
            model.Supp_CityId = Convert.ToInt32(reader["Supp_CityId"]);

        if (HasColumn(reader, "Supp_CityName") && !reader.IsDBNull(reader.GetOrdinal("Supp_CityName")))
            model.Supp_CityName = Convert.ToString(reader["Supp_CityName"]);
        else if (HasColumn(reader, "City_Name") && !reader.IsDBNull(reader.GetOrdinal("City_Name")))
            model.Supp_CityName = Convert.ToString(reader["City_Name"]);

        if (HasColumn(reader, "Supp_StateId") && !reader.IsDBNull(reader.GetOrdinal("Supp_StateId")))
            model.Supp_StateId = Convert.ToInt32(reader["Supp_StateId"]);

        if (HasColumn(reader, "Supp_StateName") && !reader.IsDBNull(reader.GetOrdinal("Supp_StateName")))
            model.Supp_StateName = Convert.ToString(reader["Supp_StateName"]);
        else if (HasColumn(reader, "State_Name") && !reader.IsDBNull(reader.GetOrdinal("State_Name")))
            model.Supp_StateName = Convert.ToString(reader["State_Name"]);

        if (HasColumn(reader, "Supp_Pincode") && !reader.IsDBNull(reader.GetOrdinal("Supp_Pincode")))
            model.Supp_Pincode = Convert.ToString(reader["Supp_Pincode"]);

        if (HasColumn(reader, "Supp_Country") && !reader.IsDBNull(reader.GetOrdinal("Supp_Country")))
            model.Supp_Country = Convert.ToString(reader["Supp_Country"]) ?? "India";

        if (HasColumn(reader, "Supp_PaymentTerms") && !reader.IsDBNull(reader.GetOrdinal("Supp_PaymentTerms")))
            model.Supp_PaymentTerms = Convert.ToString(reader["Supp_PaymentTerms"]);

        if (HasColumn(reader, "Supp_CreditLimit") && !reader.IsDBNull(reader.GetOrdinal("Supp_CreditLimit")))
            model.Supp_CreditLimit = Convert.ToDecimal(reader["Supp_CreditLimit"]);

        if (HasColumn(reader, "Supp_CreditDays") && !reader.IsDBNull(reader.GetOrdinal("Supp_CreditDays")))
            model.Supp_CreditDays = Convert.ToInt32(reader["Supp_CreditDays"]);

        if (HasColumn(reader, "Supp_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Supp_IsActive")))
            model.Supp_IsActive = Convert.ToBoolean(reader["Supp_IsActive"]);

        if (HasColumn(reader, "Supp_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("Supp_CreatedBy")))
            model.Supp_CreatedBy = Convert.ToInt32(reader["Supp_CreatedBy"]);

        if (HasColumn(reader, "Supp_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Supp_CreatedDate")))
            model.Supp_CreatedDate = Convert.ToDateTime(reader["Supp_CreatedDate"]);

        if (HasColumn(reader, "Supp_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("Supp_ModifiedBy")))
            model.Supp_ModifiedBy = Convert.ToInt32(reader["Supp_ModifiedBy"]);

        if (HasColumn(reader, "Supp_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Supp_ModifiedDate")))
            model.Supp_ModifiedDate = Convert.ToDateTime(reader["Supp_ModifiedDate"]);

        if (HasColumn(reader, "Supp_CompId") && !reader.IsDBNull(reader.GetOrdinal("Supp_CompId")))
            model.Supp_CompId = Convert.ToInt32(reader["Supp_CompId"]);

        if (HasColumn(reader, "Supp_CompanyDisplayName") && !reader.IsDBNull(reader.GetOrdinal("Supp_CompanyDisplayName")))
            model.Supp_CompanyDisplayName = Convert.ToString(reader["Supp_CompanyDisplayName"]);
        else if (HasColumn(reader, "Comp_Name") && !reader.IsDBNull(reader.GetOrdinal("Comp_Name")))
            model.Supp_CompanyDisplayName = Convert.ToString(reader["Comp_Name"]);

        if (HasColumn(reader, "Supp_BranchId") && !reader.IsDBNull(reader.GetOrdinal("Supp_BranchId")))
            model.Supp_BranchId = Convert.ToInt32(reader["Supp_BranchId"]);

        if (HasColumn(reader, "Supp_BranchName") && !reader.IsDBNull(reader.GetOrdinal("Supp_BranchName")))
            model.Supp_BranchName = Convert.ToString(reader["Supp_BranchName"]);
        else if (HasColumn(reader, "Branch_Name") && !reader.IsDBNull(reader.GetOrdinal("Branch_Name")))
            model.Supp_BranchName = Convert.ToString(reader["Branch_Name"]);

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
