namespace Billing_Software_Api.Models;

/// <summary>
/// Result returned by the SP_Employee_InsertOrUpdate stored procedure.
/// </summary>
public class EmployeeSaveResult
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
    /// The generated or updated Employee ID.
    /// </summary>
    public int Emp_Id { get; set; }
}
