namespace Billing_Software_Api.Mapping;

/// <summary>
/// Base contract or placeholder for entity-to-DTO and DTO-to-entity mapping configurations.
/// Can be extended with AutoMapper, Mapster, or manual mapper implementations.
/// </summary>
public interface IMappingProfile
{
    // Marker interface / mapping contract for module-level mapping definitions
}

/// <summary>
/// Root mapping profile placeholder for registering object transformations.
/// </summary>
public class MappingProfile : IMappingProfile
{
    // Module mappings will be registered here or in module-specific profiles
}
