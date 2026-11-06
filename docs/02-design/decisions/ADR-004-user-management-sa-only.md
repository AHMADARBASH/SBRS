# ADR-004 User management by SystemAdmin only
> Status: Accepted | Date: unknown

## Context
Accounts carry global power (role, scope); letting org/facility admins create users widens the trust surface.

## Decision
The whole Users controller requires `SystemAdminOnly` — create, update, deactivate, reset-password, roles `[BUILT]`. Self-service limits apply too: no self-deactivation, no self role change.

## Consequences
- One vetted actor for every account change; easy audit.
- Org admins cannot onboard staff themselves — a known cost accepted for v1.
- Probable bug: deactivation currently checks the caller's role (OQ-14) — the design intent is target-based.

## Alternatives considered
- Delegated user management with templates — postponed; complexity not justified yet.
- User list/read endpoints for the admin UI — proposed (`[OPEN]` OQ-10); nothing exists yet.
