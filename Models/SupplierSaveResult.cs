namespace Billing_Software_Api.Models;

public class SupplierSaveResult
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Supp_Id { get; set; }
}
