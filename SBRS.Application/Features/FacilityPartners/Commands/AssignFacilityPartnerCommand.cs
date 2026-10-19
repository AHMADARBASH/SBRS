using FluentValidation;
using MediatR;
using SBRS.Application.Common.Exceptions;
using SBRS.Application.Features.FacilityPartners.DTOs;
using SBRS.Application.Features.FacilityPartners.Mappings;
using SBRS.Domain.Entities;
using SBRS.Domain.Exceptions;
using SBRS.Domain.Interfaces;

namespace SBRS.Application.Features.FacilityPartners.Commands;

public sealed record AssignFacilityPartnerCommand(
    Guid FacilityId,
    Guid PartnerId,
    string ProjectCode,
    DateOnly StartDate) : IRequest<FacilityPartnerDto>;

public sealed class AssignFacilityPartnerCommandValidator : AbstractValidator<AssignFacilityPartnerCommand>
{
    public AssignFacilityPartnerCommandValidator()
    {
        RuleFor(x => x.FacilityId).NotEmpty();
        RuleFor(x => x.PartnerId).NotEmpty();
        RuleFor(x => x.ProjectCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StartDate).NotEmpty();
    }
}

public sealed class AssignFacilityPartnerCommandHandler(
    IFacilityRepository facilities,
    IPartnerRepository partners,
    IFacilityPartnerRepository history,
    IUnitOfWork uow) : IRequestHandler<AssignFacilityPartnerCommand, FacilityPartnerDto>
{
    public async Task<FacilityPartnerDto> Handle(AssignFacilityPartnerCommand c, CancellationToken ct)
    {
        var facility = await facilities.GetByIdAsync(c.FacilityId, ct)
                       ?? throw new NotFoundException(nameof(FacilityEntity),c.FacilityId);

        var partner = await partners.GetByIdAsync(c.PartnerId, ct)
                      ?? throw new NotFoundException(nameof(PartnerEntity),c.PartnerId);

        var current = await history.GetCurrentAsync(c.FacilityId, ct);
        if (current is not null)
        {
            if (c.StartDate <= current.StartDate)
                throw new DomainException(
                    $"The new start date must be later than the current assignment start date ({current.StartDate:yyyy-MM-dd}).");

            // The previous assignment ends the day before the new one starts.
            current.Close(c.StartDate.AddDays(-1));
        }

        facility.ChangePartner(partner.Id);

        var entry = new FacilityPartnerEntity(facility, partner, c.StartDate, c.ProjectCode);
        await history.AddAsync(entry, ct);

        await uow.SaveChangesAsync(ct);

        return entry.ToDto();
    }
}