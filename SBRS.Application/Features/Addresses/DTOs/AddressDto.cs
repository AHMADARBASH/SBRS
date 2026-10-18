namespace SBRS.Application.Features.Addresses.DTOs;

public record AddressDto(
    Guid Id,
    Guid AreaId,
    string ArName,
    string EnName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);