# ADR-013 Migration from SQL Server to PostgreSQL
> Status: Accepted | Date: 2026-10-27

## Context
Cost and licensing constraints, plus native JSONB/citext features, moved the project off SQL Server early (before production data accumulated).

## Decision
Switch to PostgreSQL with Npgsql EF Core 10: snake_case column naming on all project tables (`name_property` → `name_property`); the ASP.NET Identity tables keep their PascalCase names (see OQ-11). The `citext` extension is enabled and applied to code-like columns so far (`facilities.code`, `beneficiaries.full_code`). The database was **restarted from scratch**: fresh schema + migrations and re-seeding — no data migration was performed `[ASSUMED: fresh start accepted]`.

## Consequences
- Case-insensitive code matching for free (unique `citext` + queries without `LOWER()`).
- Mixed table naming until a possible Identity rename (OQ-11).
- Unique violations surface as SQLSTATE `23505` → mapped to HTTP 409.

## Alternatives considered
- Staying on SQL Server — rejected: licensing/hosting cost.
- Migrating schema + data wholesale — not needed at that stage (system not yet live).
