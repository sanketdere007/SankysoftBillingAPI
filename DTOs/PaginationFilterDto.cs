namespace Billing_Software_Api.DTOs;

/// <summary>
/// DTO for handling pagination, sorting, and filtering parameters in API requests.
/// </summary>
public class PaginationFilterDto
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 10 : value);
    }

    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool IsAscending { get; set; } = true;
    public bool? IsActive { get; set; }
}
