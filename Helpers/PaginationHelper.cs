using Billing_Software_Api.Common;

namespace Billing_Software_Api.Helpers;

/// <summary>
/// Helper class for constructing paginated responses.
/// </summary>
public static class PaginationHelper
{
    public static PagedResponse<T> CreatePagedResponse<T>(
        IReadOnlyList<T> data,
        int pageNumber,
        int pageSize,
        int totalRecords,
        string message = "Paginated data retrieved successfully.")
    {
        return new PagedResponse<T>(data, pageNumber, pageSize, totalRecords, message);
    }
}
