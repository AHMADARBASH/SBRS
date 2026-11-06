# SBRS Environments and Test Data
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

Placeholders only — **no real URLs, credentials or keys in this document**.

## 1. Environments

| Environment | Base URL (placeholder) | Purpose | Notes |
|---|---|---|---|
| Development (local) | `<DEV_BASE_URL>` — default launch profile `http://localhost:5261` (HTTPS `https://localhost:7105`) | daily development | Swagger UI enabled; secrets via user secrets |
| Test/UAT | `<UAT_BASE_URL>` `[OPEN]` — no UAT environment is defined anywhere in the repo | pre-release verification | same deploy pipeline as prod, separate DB `[ASSUMED]` |
| Production | `<PROD_BASE_URL>` `[ASSUMED]` | live system | Linux + systemd + PostgreSQL; secrets via `EnvironmentFile` (see [deployment-linux](../04-operations/deployment-linux.md)) |

There is exactly one deployment pipeline (`azure-pipelines.yml`) that publishes to the production path `<PROD_PATH>` — there is no separate staging environment in the repo `[BUILT]`.

## 2. What is seeded automatically

The **only** seeder in the codebase is the Identity one, which runs at every API startup `[BUILT]`:

1. Creates the 6 roles from `Roles.All` if missing: `SystemAdmin`, `OrganizationAdmin`, `BranchAdmin`, `ProjectCoordinator`, `FacilityAdmin`, `DataEntry`.
2. Creates the first **SystemAdmin** user if it does not exist, using:
   - `SeedAdmin:UserName`, `SeedAdmin:Password` (user secrets in Development; `SeedAdmin__UserName`, `SeedAdmin__Password` in the production EnvironmentFile) — names only, never store values here.
   - The account is created with FirstName/LastName "System Administrator", active, not mobile.
   - Skips silently with a log warning when the credentials are not configured, and does nothing when the user already exists.

**Lookup data is NOT seeded by any script in the repo** — these tables start empty `[OPEN]` (OQ-21):

| Data | Needed for | Intended source |
|---|---|---|
| countries / cities / regions / districts / areas | facility codes, addresses, register screen | OCHA Syria admin boundaries `[PLANNED]` `[ASSUMED]` (seed script not in repo — OQ-21) |
| nationalities | register screen | `[OPEN]` |
| identity_types | register screen | `[OPEN]` (fixed codes in `IdentityTypeCodes.cs`) |
| facility_types | facility creation (their `code` feeds the facility code) | `[OPEN]` |
| partners | facility creation | `[OPEN]` |
| accommodation_types / disability_types / education_types | future BeneficiaryProfile | `[OPEN]` |

Until a seed story exists (OQ-21), test environments must fill these tables manually through the admin API endpoints (all SystemAdmin-only).

## 3. Suggested test accounts

Created by SA via `POST api/Users/CreateUser` after the base SystemAdmin exists. Names only; set real passwords in your environment `[ASSUMED]`.

| Account userName (suggested) | Role | Scope | IsMobile | Used to verify |
|---|---|---|---|---|
| `sysadmin` | SystemAdmin | — (never bind) | false | global reads, structure admin, users, match review across facilities |
| `orgadmin.dam` | OrganizationAdmin | one organization (Damascus) | false | org-scoped read/404, facility add in own org |
| `branchadmin.dam` | BranchAdmin | branchId `[PLANNED]` — cannot be set yet | false | today: empty scoped results (OQ-28) |
| `projcoord.dam` | ProjectCoordinator | assigned facilities `[PLANNED]` — cannot be set yet | false | today: empty scoped results (OQ-28) |
| `facadmin.h001` | FacilityAdmin | one facility | false | match review queue, facility list |
| `dataentry.h001` | DataEntry | same facility as `facadmin.h001` | false | register, search, list |
| `dataentry.mob` | DataEntry | one facility | **true** | mobile flows, `is_mobile` claim |
| `locked.user` | any | — | false | lockout test — ⚠ currently never locks (`LockoutEnabled` never set true, OQ-36) |
| `inactive.user` | any | — | false | login/refresh refusal after deactivation |

## 4. Minimal viable test dataset

To exercise the beneficiary flow end-to-end you need, in order:

1. Roles (auto at startup).
2. SystemAdmin (auto at startup).
3. One organization → one branch → facility type → partner → geography rows for one city → one address → **one facility** (its `code` is needed by beneficiary codes).
4. Nationalities + identity types (at least `NATIONAL_ID` and `UNREGISTERED`).
5. Two users per facility (FA + DE) from section 3.
6. The known tricky cases to keep as fixtures:
   - two beneficiaries differing only by disambiguator (same 5-field key) `[BUILT: Disambiguator allows exact-key duplicates under review]`
   - twin pattern (same mother/last/birth date, different first name)
   - Arabic name variants (alef forms, ى/ي, ة/ه, diacritics, tatweel)
   - NATIONAL_ID with Arabic-Indic digits (must be converted, then match the 10-digit regex) `[BUILT]`
   - UNREGISTERED with a non-empty value (must fail)
