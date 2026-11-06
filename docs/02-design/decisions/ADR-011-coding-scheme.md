# ADR-011 Facility and beneficiary coding scheme
> Status: Accepted | Date: 2026-10-28 (code generator services)

## Context
Beneficiaries need stable readable references usable in person and on paper; a global auto counter would leak volume and follow no geography.

## Decision
Facility code = `{city c_code}-{facility type code}-{3-digit sequence}` where the sequence is derived at insert time (count of facilities of that city + 1 — rare event, no counter column); unique `citext` index guards races with retry-on-violation. Beneficiary code = per-facility sequence allocated under a `SELECT … FOR UPDATE` lock on the facility row (concurrency-safe, gap-free per facility); **FullCode = `{facility code}-{beneficiary code}`** unique system-wide. City `c_code`s (DAM, ALP, RUD, …) come from the cities seed `[OPEN: seed missing, OQ-21]`.

## Consequences
- Codes are human-local: `DAM-PHC-001-57` reads as "Damascus, Primary Health Care, facility 1, person 57".
- No central counters to migrate or reset; sequence spacing is inherent.
- Writing concurrency handled by DB locks, not extra tables.

## Alternatives considered
- Global DB sequence for beneficiaries — replaced by per-facility allocation (BR-BEN-06).
- Server-side counters table — rejected: migration overhead on a fresh system.
