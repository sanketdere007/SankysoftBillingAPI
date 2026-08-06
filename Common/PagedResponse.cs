namespace Billing_Software_Api.Common;

/// <summary>
/// Standardized paginated API response wrapper.
/// </summary>
/// <typeparam name="T">Type of items in the list.</typeparam>
public class PagedResponse<T> : ApiResponse<IReadOnlyList<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResponse()
    {
    }

    public PagedResponse(
        IReadOnlyList<T> data,
        int pageNumber,
        int pageSize,
        int totalRecords,
        string message = "Paginated data retrieved successfully.",
        int statusCode = 200)
        : base(true, message, statusCode, data)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize < 1 ? 10 : pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);
    }

    public static PagedResponse<T> Create(
        IReadOnlyList<T> data,
        int pageNumber,
        int pageSize,
        int totalRecords,
        string message = "Paginated data retrieved successfully.")
    {
        return new PagedResponse<T>(data, pageNumber, pageSize, totalRecords, message);
    }
}
