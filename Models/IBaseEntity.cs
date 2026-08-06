namespace Billing_Software_Api.Models;

/// <summary>
/// Interface contract for all domain entities.
/// </summary>
public interface IBaseEntity
{
    int Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
    bool IsActive { get; set; }
    bool IsDeleted { get; set; }
}
