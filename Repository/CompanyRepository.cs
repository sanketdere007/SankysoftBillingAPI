using System.Data;
using Billing_Software_Api.Data;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repository.Interfaces;
using Microsoft.Data.SqlClient;

namespace Billing_Software_Api.Repository;

/// <summary>
/// ADO.NET repository implementation for Company data operations using Stored Procedures.
/// </summary>
public class CompanyRepository : ICompanyRepository
{
    private readonly DbHelper _dbHelper;
    private readonly ILogger<CompanyRepository> _logger;

    public CompanyRepository(DbHelper dbHelper, ILogger<CompanyRepository> logger)
    {
        _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Fetches all company records from SQL Server using SP_Company_GetAll stored procedure.
    /// </summary>
    public async Task<ApiResponse<List<CompanyListModel>>> GetAllCompaniesAsync(CompanyFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var parameters = new[]
            {
                DbHelper.CreateParameter("@Comp_IsActive", filter?.Comp_IsActive.HasValue == true ? filter.Comp_IsActive.Value : DBNull.Value, SqlDbType.Bit)
            };

            var companies = await _dbHelper.ExecuteStoredProcedureAsync(
                procedureName: "dbo.SP_Company_GetAll",
                parameters: parameters,
                mapReaderFunc: async reader =>
                {
                    var list = new List<CompanyListModel>();
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        list.Add(MapCompanyFromReader(reader));
                    }
                    return list;
                },
                cancellationToken: cancellationToken);

            return ApiResponse<List<CompanyListModel>>.SuccessResult(
                data: companies,
                message: $"Successfully retrieved {companies.Count} company record(s).");
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL Server error occurred while fetching companies using SP_Company_GetAll.");
            return ApiResponse<List<CompanyListModel>>.FailureResult(
                message: "Unable to retrieve company list from database.",
                error: sqlEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while fetching companies.");
            return ApiResponse<List<CompanyListModel>>.FailureResult(
                message: "An unexpected error occurred while fetching companies.",
                error: ex.Message);
        }
    }

    /// <summary>
    /// Helper method to safely map SqlDataReader columns to a CompanyListModel instance.
    /// </summary>
    public static CompanyListModel MapCompanyFromReader(SqlDataReader reader)
    {
        var model = new CompanyListModel();

        if (HasColumn(reader, "Comp_Id") && !reader.IsDBNull(reader.GetOrdinal("Comp_Id")))
            model.Comp_Id = Convert.ToInt32(reader["Comp_Id"]);

        if (HasColumn(reader, "Comp_Name") && !reader.IsDBNull(reader.GetOrdinal("Comp_Name")))
            model.Comp_Name = Convert.ToString(reader["Comp_Name"]) ?? string.Empty;

        if (HasColumn(reader, "Comp_ContactPerson") && !reader.IsDBNull(reader.GetOrdinal("Comp_ContactPerson")))
            model.Comp_ContactPerson = Convert.ToString(reader["Comp_ContactPerson"]);

        if (HasColumn(reader, "Comp_MobileNo") && !reader.IsDBNull(reader.GetOrdinal("Comp_MobileNo")))
            model.Comp_MobileNo = Convert.ToString(reader["Comp_MobileNo"]);

        if (HasColumn(reader, "Comp_AlternateMobileNo") && !reader.IsDBNull(reader.GetOrdinal("Comp_AlternateMobileNo")))
            model.Comp_AlternateMobileNo = Convert.ToString(reader["Comp_AlternateMobileNo"]);

        if (HasColumn(reader, "Comp_Email") && !reader.IsDBNull(reader.GetOrdinal("Comp_Email")))
            model.Comp_Email = Convert.ToString(reader["Comp_Email"]);

        if (HasColumn(reader, "Comp_Website") && !reader.IsDBNull(reader.GetOrdinal("Comp_Website")))
            model.Comp_Website = Convert.ToString(reader["Comp_Website"]);

        if (HasColumn(reader, "Comp_GSTNo") && !reader.IsDBNull(reader.GetOrdinal("Comp_GSTNo")))
            model.Comp_GSTNo = Convert.ToString(reader["Comp_GSTNo"]);

        if (HasColumn(reader, "Comp_PANNo") && !reader.IsDBNull(reader.GetOrdinal("Comp_PANNo")))
            model.Comp_PANNo = Convert.ToString(reader["Comp_PANNo"]);

        if (HasColumn(reader, "Comp_Address") && !reader.IsDBNull(reader.GetOrdinal("Comp_Address")))
            model.Comp_Address = Convert.ToString(reader["Comp_Address"]);

        if (HasColumn(reader, "Comp_Area") && !reader.IsDBNull(reader.GetOrdinal("Comp_Area")))
            model.Comp_Area = Convert.ToString(reader["Comp_Area"]);

        if (HasColumn(reader, "Comp_City") && !reader.IsDBNull(reader.GetOrdinal("Comp_City")))
            model.Comp_City = Convert.ToString(reader["Comp_City"]);

        if (HasColumn(reader, "Comp_State") && !reader.IsDBNull(reader.GetOrdinal("Comp_State")))
            model.Comp_State = Convert.ToString(reader["Comp_State"]);

        if (HasColumn(reader, "Comp_Pincode") && !reader.IsDBNull(reader.GetOrdinal("Comp_Pincode")))
            model.Comp_Pincode = Convert.ToString(reader["Comp_Pincode"]);

        if (HasColumn(reader, "Comp_Country") && !reader.IsDBNull(reader.GetOrdinal("Comp_Country")))
            model.Comp_Country = Convert.ToString(reader["Comp_Country"]);

        if (HasColumn(reader, "Comp_Logo") && !reader.IsDBNull(reader.GetOrdinal("Comp_Logo")))
            model.Comp_Logo = Convert.ToString(reader["Comp_Logo"]);

        if (HasColumn(reader, "Comp_IsActive") && !reader.IsDBNull(reader.GetOrdinal("Comp_IsActive")))
            model.Comp_IsActive = Convert.ToBoolean(reader["Comp_IsActive"]);

        if (HasColumn(reader, "Comp_CreatedBy") && !reader.IsDBNull(reader.GetOrdinal("Comp_CreatedBy")))
            model.Comp_CreatedBy = Convert.ToInt32(reader["Comp_CreatedBy"]);

        if (HasColumn(reader, "Comp_CreatedDate") && !reader.IsDBNull(reader.GetOrdinal("Comp_CreatedDate")))
            model.Comp_CreatedDate = Convert.ToDateTime(reader["Comp_CreatedDate"]);

        if (HasColumn(reader, "Comp_ModifiedBy") && !reader.IsDBNull(reader.GetOrdinal("Comp_ModifiedBy")))
            model.Comp_ModifiedBy = Convert.ToInt32(reader["Comp_ModifiedBy"]);

        if (HasColumn(reader, "Comp_ModifiedDate") && !reader.IsDBNull(reader.GetOrdinal("Comp_ModifiedDate")))
            model.Comp_ModifiedDate = Convert.ToDateTime(reader["Comp_ModifiedDate"]);

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
