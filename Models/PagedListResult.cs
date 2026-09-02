namespace Billing_Software_Api.Models;

/// <summary>
/// Paginated list wrapper for stored-procedure GetAll results.
/// </summary>
public class PagedListResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
