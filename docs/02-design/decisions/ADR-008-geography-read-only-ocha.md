# ADR-008 Geography as read-only lookups seeded from OCHA Syria boundaries
> Status: Accepted | Date: unknown
> Seeding status: `[PLANNED]` — no seed script exists in the repo (OQ-21)

## Context
Administrative divisions change rarely and must not drift between facilities; inventing them invites inconsistency.

## Decision
Country → City → Region → District → Area are read-only lookups (GET-only API, no write endpoints) filled once from the OCHA Syria admin0–3 + populated-places datasets; each row's `Code` is the OCHA pcode. Areas additionally carry `CityId` for city-filtered pickers. Human-readable addresses are the only user-created geography, shared system-wide (BR-GEO-03).

## Consequences
- Every facility/beneficiary references the same authoritative codes; facility code prefixes derive from the city `c_code`.
- Updates require a re-seed run (idempotent) — no runtime editing allowed.
- If OCHA revises boundaries, existing pcodes keep working for historical rows.

## Alternatives considered
- Free-text addresses only — rejected: no reliable coding, no cascading selects.
- Admin-managed geography CRUD — rejected: drift risk.
