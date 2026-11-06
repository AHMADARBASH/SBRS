# SBRS API Changelog
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

This file records **API-breaking and client-visible changes**. The default branch's first published contract is the baseline; earlier history is summarized as "first release" below.

Format: [Keep a Changelog](https://keepachangelog.com/) style — sections `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`; mark breaking entries with **BREAKING**. Dates use the merge date from git history.

## Unreleased

Documentation release: the whole `docs/` tree was created; no API code changed while writing it.

- Added — this documentation set; see [README](../README.md).

## 2026-10-08 → 2026-11-05 (initial development, from git history)

### Added

- Clean-architecture API skeleton (Domain / Application / Infrastructure / Web) with MediatR, FluentValidation, EF Core, PostgreSQL.
- Authentication: login, access JWT (240 min), rotating single-use refresh tokens (30 days, family reuse detection), logout / logout-all, `me` endpoint (2026-10-13/14).
- HttpOnly refresh-cookie flow for web clients with `X-Client-Type: web` header (2026-10-26).
- Roles: SystemAdmin, OrganizationAdmin, BranchAdmin, ProjectCoordinator, FacilityAdmin, DataEntry; fallback authorization policy.
- Geography feature (country/city/region/district/area cascading reads) (2026-10-11/12).
- Organizations, branches, addresses, facilities CRUD with scoping (2026-10-10 → 10-18).
- Users management (create/update/deactivate/reset-password, roles endpoint) (2026-10-19, roles GET 2026-11-02).
- Facility types, partners, facility-partner history, accommodation/disability/education types, nationalities, identity types admin (2026-10-19 → 10-21).
- Identity type codes extended to 30 chars (2026-10-22).
- Beneficiary feature: register with identity documents + per-facility code generation, cross-facility search, facility list (pagination), match detection (exact key + score), facility-admin review (ConfirmSame / ConfirmNotAMatch), soft delete (2026-10-25 → 11-05).
- Facility code generator service (2026-10-28); per-facility beneficiary code generation (2026-10-28).
- Migration from SQL Server to PostgreSQL (snake_case, citext) (2026-10-27).
- CI/CD: Azure Pipelines, deploy to Linux, Swagger enabled in production (2026-11-03/04).
- CORS with configured origins + credentials (2026-11-04).

### Fixed / notable behavior

- `is_mobile` claim and user field now flows through tokens.
- Identity documents are included in the matching algorithm (`SameIdentity` reason, exact match) (2026-11-05).

### Known current gaps (see [OPEN-QUESTIONS](../OPEN-QUESTIONS.md))

- No user list/read endpoints (OQ-10); beneficiary register policy is DataEntry-only (OQ-12); deactivation endpoint bug (OQ-14); several `includeInactive` parameters ignored (OQ-19); lookup seed data missing (OQ-21).
