using Billing_Software_Api.Common;
using Billing_Software_Api.DTOs;

namespace Billing_Software_Api.Business.Interfaces;

/// <summary>
/// Base generic interface contract for business logic services.
/// </summary>
/// <typeparam name="TResponseDto">DTO type returned for read operations.</typeparam>
/// <typeparam name="TCreateDto">DTO type used for create operations.</typeparam>
/// <typeparam name="TUpdateDto">DTO type used for update operations.</typeparam>
public interface IBaseBusinessService<TResponseDto, in TCreateDto, in TUpdateDto>
    where TResponseDto : BaseDto
{
    Task<ApiResponse<TResponseDto?>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyList<TResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PagedResponse<TResponseDto>> GetPagedAsync(PaginationFilterDto filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<TResponseDto>> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> UpdateAsync(int id, TUpdateDto updateDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
