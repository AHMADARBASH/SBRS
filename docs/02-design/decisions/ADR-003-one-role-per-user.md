# ADR-003 One role per user; scope stored on the user
> Status: Accepted | Date: unknown

## Context
SARC staff have one job function each; permissions follow the role plus the org/facility they work at, which the system can derive — admins must not hand-wire rights.

## Decision
Exactly one Identity role per user (`AspNetUserRoles` enforced by service code). Scope lives on `ApplicationUser`: `OrganizationId` and `FacilityId` (nullable), `IsMobile` flag; `BranchId` is `[PLANNED]`. OrganizationId is derived from the facility/branch, never entered by hand. Claims `organization_id`/`facility_id` (branch_id planned) flow inside the access token; repositories scope reads by those claims.

## Consequences
- Role × scope is atomic per user; no compound-role ambiguity.
- ProjectCoordinator (multi-facility) needs an assigned-facilities table before it can exist usefully (OQ-04).
- Changing role/scope revokes refresh tokens so claims cannot outlive rights `[BUILT]`.

## Alternatives considered
- Claim-level permissions per user — rejected: too granular for this team size.
- Multiple roles per user — rejected: ambiguous scope resolution.
