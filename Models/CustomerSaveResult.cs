namespace Billing_Software_Api.Models;

/// <summary>
/// Result returned by the SP_Customer_InsertOrUpdate stored procedure.
/// </summary>
public class CustomerSaveResult
{
    /// <summary>
    /// Status indicating whether the insert or update succeeded (true/1) or failed (false/0).
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Informational message returned by the stored procedure.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The generated or updated Customer ID.
    /// </summary>
    public int Cust_Id { get; set; }

    /// <summary>
    /// The generated or existing Customer Code (e.g., CUST000001).
    /// </summary>
    public string? Cust_Code { get; set; }
}
