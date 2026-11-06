# ADR Index
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

| ID | Title | Status | Date |
|---|---|---|---|
| [ADR-001](ADR-001-clean-architecture.md) | Clean architecture with MediatR, FluentValidation, no AutoMapper | Accepted | 2026-10-08 (project start) |
| [ADR-002](ADR-002-mediatr-pinned.md) | MediatR pinned to the free 12.x line | Accepted | date unknown |
| [ADR-003](ADR-003-one-role-per-user.md) | One role per user; scope stored on the user | Accepted | date unknown |
| [ADR-004](ADR-004-user-management-sa-only.md) | User management by SystemAdmin only | Accepted | date unknown |
| [ADR-005](ADR-005-token-design.md) | JWT access token 4 h + rotating single-use refresh token 30 d with reuse detection | Accepted | 2026-10-13/14 (auth feature) |
| [ADR-006](ADR-006-404-not-403.md) | Out-of-scope records return 404, not 403 | Accepted | date unknown |
| [ADR-007](ADR-007-soft-delete-immutable-parents.md) | Soft delete through `IsActive`; parent ids immutable | Accepted | date unknown |
| [ADR-008](ADR-008-geography-read-only-ocha.md) | Geography as read-only lookups seeded from OCHA Syria boundaries | Accepted (seed still `[PLANNED]`, OQ-21) | date unknown |
| [ADR-009](ADR-009-beneficiary-unique-person.md) | Beneficiary is one global person; five-field match key with normalised Arabic names | Accepted | 2026-10-25 (beneficiary feature) |
| [ADR-010](ADR-010-search-by-code-or-names.md) | Search by FullCode or first + last name across facilities | Accepted | 2026-10-25 |
| [ADR-011](ADR-011-coding-scheme.md) | Facility and beneficiary coding scheme | Accepted | 2026-10-28 (code generators) |
| [ADR-012](ADR-012-facility-partner-history.md) | Facility partner history table (`facilities_partners`) | Accepted | 2026-10-19 |
| [ADR-013](ADR-013-postgres-migration.md) | Migration from SQL Server to PostgreSQL | Accepted | 2026-10-27 |
| [ADR-014](ADR-014-refresh-cookie.md) | Refresh token delivery: HttpOnly cookie for web, body for mobile | Accepted — `[BUILT]` (2026-10-26) | 2026-10-26 |
| [ADR-015](ADR-015-pharmacy-role-separation.md) | Pharmacy staff must never register beneficiaries (separate pharmacy role) | Proposed `[OPEN]` (OQ-01/OQ-02) | date unknown |

New ADRs use the template below and smooth numbering (never renumber existing ones).

Template:

```markdown
# ADR-XXX Title
> Status: Accepted | Date: YYYY-MM-DD

## Context
## Decision
## Consequences
## Alternatives considered
```
