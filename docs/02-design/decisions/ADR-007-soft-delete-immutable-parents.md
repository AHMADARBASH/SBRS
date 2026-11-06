# ADR-007 Soft delete through IsActive; parent ids immutable
> Status: Accepted | Date: unknown

## Context
Registration data must be auditable and never lost; restructuring parents would orphan history.

## Decision
Nothing is hard-deleted through the API `[BUILT]`. Deactivation flips `IsActive = false` (beneficiaries included; FacilityTypes are insert-only with no IsActive). Parent references (org on branch, branch on facility, area on address, facility on beneficiary) cannot change after creation; Update contracts only carry safe fields. Domain guards block updates of inactive rows and double deactivation.

## Consequences
- Full history retained with audit stamps.
- Activation/reactivation endpoints are deliberately absent so far `[ASSUMED]` (one-way deactivation).
- Deactivations must be guarded (active children) where relevant — partially implemented: branch and address guards incomplete (OQ-16, OQ-17).

## Alternatives considered
- Hard delete with EF cascade — rejected: destroys audit trail and match history.
