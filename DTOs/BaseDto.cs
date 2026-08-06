namespace Billing_Software_Api.DTOs;

/// <summary>
/// Base Data Transfer Object with standard properties.
/// </summary>
public abstract class BaseDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
