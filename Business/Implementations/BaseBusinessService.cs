using Billing_Software_Api.Business.Interfaces;
using Billing_Software_Api.Common;
using Billing_Software_Api.DTOs;
using Billing_Software_Api.Models;
using Billing_Software_Api.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Billing_Software_Api.Business.Implementations;

/// <summary>
/// Abstract base business service providing standard business workflows, repository interactions, and response mapping.
/// </summary>
/// <typeparam name="TEntity">Domain entity deriving from BaseEntity.</typeparam>
/// <typeparam name="TResponseDto">Response DTO deriving from BaseDto.</typeparam>
/// <typeparam name="TCreateDto">Create DTO.</typeparam>
/// <typeparam name="TUpdateDto">Update DTO.</typeparam>
public abstract class BaseBusinessService<TEntity, TResponseDto, TCreateDto, TUpdateDto>
    : IBaseBusinessService<TResponseDto, TCreateDto, TUpdateDto>
    where TEntity : BaseEntity
    where TResponseDto : BaseDto
{
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IGenericRepository<TEntity> _repository;
    protected readonly ILogger _logger;

    protected BaseBusinessService(IUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _repository = _unitOfWork.Repository<TEntity>();
    }

    public virtual async Task<ApiResponse<TResponseDto?>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
        {
            return ApiResponse<TResponseDto?>.FailureResult($"Record with ID {id} was not found.", statusCode: 404);
        }

        var dto = MapToDto(entity);
        return ApiResponse<TResponseDto?>.SuccessResult(dto, "Record retrieved successfully.");
    }

    public virtual async Task<ApiResponse<IReadOnlyList<TResponseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        var dtos = entities.Select(MapToDto).ToList();
        return ApiResponse<IReadOnlyList<TResponseDto>>.SuccessResult(dtos, "Records retrieved successfully.");
    }

    public virtual async Task<PagedResponse<TResponseDto>> GetPagedAsync(PaginationFilterDto filter, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            predicate: null,
            orderBy: null,
            cancellationToken: cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        return PagedResponse<TResponseDto>.Create(dtos, filter.PageNumber, filter.PageSize, totalCount);
    }

    public virtual async Task<ApiResponse<TResponseDto>> CreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default)
    {
        var validationResult = await ValidateCreateAsync(createDto, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return ApiResponse<TResponseDto>.FailureResult(validationResult.Message, validationResult.Errors, 400);
        }

        var entity = MapToEntity(createDto);
        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var responseDto = MapToDto(entity);
        return ApiResponse<TResponseDto>.SuccessResult(responseDto, "Record created successfully.", 201);
    }

    public virtual async Task<ApiResponse<bool>> UpdateAsync(int id, TUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _repository.GetByIdAsync(id, cancellationToken);
        if (existingEntity == null)
        {
            return ApiResponse<bool>.FailureResult($"Record with ID {id} was not found.", statusCode: 404);
        }

        var validationResult = await ValidateUpdateAsync(id, updateDto, cancellationToken);
        if (!validationResult.IsSuccess)
        {
            return ApiResponse<bool>.FailureResult(validationResult.Message, validationResult.Errors, 400);
        }

        MapToUpdatedEntity(updateDto, existingEntity);
        await _repository.UpdateAsync(existingEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, "Record updated successfully.");
    }

    public virtual async Task<ApiResponse<bool>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existingEntity = await _repository.GetByIdAsync(id, cancellationToken);
        if (existingEntity == null)
        {
            return ApiResponse<bool>.FailureResult($"Record with ID {id} was not found.", statusCode: 404);
        }

        await _repository.DeleteAsync(existingEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, "Record deleted successfully.");
    }

    // Abstract mapping hooks to be implemented by domain business services
    protected abstract TResponseDto MapToDto(TEntity entity);
    protected abstract TEntity MapToEntity(TCreateDto createDto);
    protected abstract void MapToUpdatedEntity(TUpdateDto updateDto, TEntity targetEntity);

    // Business validation hooks with default passing results
    protected virtual Task<Result> ValidateCreateAsync(TCreateDto createDto, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Success());
    }

    protected virtual Task<Result> ValidateUpdateAsync(int id, TUpdateDto updateDto, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Success());
    }
}
