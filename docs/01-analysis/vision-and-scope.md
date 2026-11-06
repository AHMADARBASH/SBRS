# SBRS Vision and Scope
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

## 1. Purpose

SBRS (Syria Beneficiary Registration System) is a unified beneficiary registry for the Syrian Arab Red Crescent (SARC). It answers one core question: **"has this person been registered before, anywhere in our network?"** — so that services are not duplicated, statistics stay honest, and each facility codes its beneficiaries consistently.

The system runs as a single ASP.NET Core Web API backed by PostgreSQL, consumed by a web frontend (office users) and a mobile app (field workers over mixed connectivity).

## 2. Users and actors

| Actor | How they use SBRS |
|---|---|
| SystemAdmin (HQ IT) | owns the structure: organizations, branches, facilities, partners, lookups, user accounts |
| OrganizationAdmin | manages their organization's branches, facilities, addresses; proposes medicines `[PLANNED]` |
| BranchAdmin | oversight of a branch's facilities `[PLANNED]`: scope machinery not built yet |
| ProjectCoordinator | oversight of their assigned facilities `[PLANNED]`: assigned-facilities table not built |
| FacilityAdmin | runs one facility: reviews duplicate-match cases, views the facility roster |
| DataEntry (field/office) | searches and registers beneficiaries at one facility (also on mobile) |
| Beneficiary (the person registered) | never an API user; exists as a data record `[CONFIRMED]` |

## 3. In scope for v1 `[BUILT]` unless tagged

| Module | Content |
|---|---|
| Authentication | username login, JWT + rotating refresh tokens, web cookie flow |
| Organization structure | organizations → branches → facilities, facility types, partners + partner history |
| Addresses & geography | read-only OCHA-boundary lookups (country→city→region→district→area) + shared addresses |
| Users | account lifecycle (create/update/deactivate/reset) by SystemAdmin only |
| Beneficiaries | search-first registration, per-facility codes, Arabic name matching, dual identity documents, match review by FacilityAdmin, soft delete |
| Lookups | nationalities, identity types, accommodation/disability/education types `[BUILT: APIs]` (consumption `[PLANNED]`, OQ-32) |

## 4. Out of scope for v1 `[PLANNED]`

- Clinics and doctors per facility (UC-11), medicines catalogue (UC-12), pharmacy transactions (UC-13), reports (UC-14) — designed, not built.
- BranchAdmin / ProjectCoordinator scoping (branch_id claim, assigned facilities) `[PLANNED]`.
- BeneficiaryProfile (education, accommodation, disabilities 1:1 extension) `[PLANNED]`.
- Enrollment of an already-registered person in an additional facility (`FacilityBeneficiary`) `[PLANNED]`.
- EducationIssue lookup `[OPEN]` (OQ-07).
- Search audit + rate limiting `[PLANNED]`; refresh-token cleanup job `[PLANNED]`.
- Mass data import from legacy paper/dbf records `[OPEN]`.
- Any UI — SBRS itself is API-only.

## 5. Constraints

| Constraint | Consequence |
|---|---|
| **Arabic-first users** | names are Arabic; the system normalizes Arabic text before comparing (alef forms, taa marbuta, diacritics, tatweel, Arabic digits) `[BUILT]` |
| **Healthcare data privacy** | identity document numbers and mother names are sensitive; cross-facility search hides the document value; logging must never capture identity values `[ASSUMED]` (OQ/BR-BEN-17) |
| **Mixed connectivity (field teams)** | mobile users (`is_mobile`) work offline-tolerant; short searches, small pages (30), token refresh survives connectivity loss |
| **One role per user, one facility per DataEntry** | scoping is server-side; the client cannot widen it `[BUILT]` |
| **PostgreSQL, snake_case schema** | EF Core + Npgsql with the `citext` extension; the code columns `facilities.code` and `beneficiaries.full_code` are case-insensitive `citext` `[BUILT]`; deployed on a single Linux server |
| **Small team, one backend owner** | clean architecture chosen for long-term maintainability over pure speed of delivery (ADR-001) |

## 6. Success criteria (proposed) `[ASSUMED]`

- A person cannot be registered twice without a deliberate human decision (match review).
- Every beneficiary is reachable by a unique, readable full code derived from the facility.
- A facility admin can reconcile the pending-match queue in minutes, not days.
- Zero identity document values in any log file.

## 7. Related documents

- Glossary: [glossary.md](glossary.md)
- Roles and permissions: [roles-and-permissions.md](roles-and-permissions.md)
- Business rules: [business-rules.md](business-rules.md)
- Use cases: [use-cases/README.md](use-cases/README.md)
- Design: [../02-design/architecture.md](../02-design/architecture.md)
