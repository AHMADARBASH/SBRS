namespace SBRS.Application.Features.Branches.DTOs;

public record BranchDto(
     Guid Id,
     string Code,
     string ArName,
     string EnName,
     Guid organizationId,
     bool IsActive,
     DateTime CreatedAt,
     DateTime? UpdatedAt);
