# SBRS Data Dictionary — Domain Tables
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

Generated from the EF Core model snapshot (`AppDbContextModelSnapshot.cs`) — real PostgreSQL names. Identity/`AspNet*` tables and `refresh_tokens` live in [data-dictionary-identity.md](data-dictionary-identity.md).

**Audit columns** (present on every table below, omitted from each listing): `id uuid` PK (except identity link tables), `created_at timestamptz`, `created_by uuid NULL`, `updated_at timestamptz NULL`, `updated_by uuid NULL` `[BUILT]`.

Conventions: snake_case columns; identity tables are the PascalCase exception (OQ-11); `citext` = case-insensitive text (PostgreSQL extension).

## beneficiaries

| Column | Type | Null | Key / Notes |
|---|---|---|---|
| `first_name` | varchar(100) | NOT NULL | as entered |
| `father_name` | varchar(100) | NOT NULL | second name |
| `last_name` | varchar(100) | NOT NULL | |
| `mother_name` | varchar(100) | NOT NULL | her first name only |
| `normalized_first_name` | varchar(100) | NOT NULL | `ArabicTextNormalizer` output |
| `normalized_father_name` | varchar(100) | NOT NULL | |
| `normalized_last_name` | varchar(100) | NOT NULL | |
| `normalized_mother_name` | varchar(100) | NOT NULL | |
| `birth_date` | date | NULL (required in practice) | `0001-01-01` possible — OQ-18 |
| `is_birth_date_estimated` | boolean | NOT NULL | true ⇒ year-only rule |
| `gender` | boolean | NOT NULL | **true = male, false = female** |
| `code` | integer | NOT NULL | per-facility sequence |
| `full_code` | citext(50) | NOT NULL | **unique** `ix_beneficiaries_full_code` |
| `match_key` | char(64) | NOT NULL | SHA-256 hex, `unicode = false` |
| `disambiguator` | integer | NOT NULL | 0,1,2… for same-key records |
| `match_review_status` | smallint | NOT NULL | 0 NoMatchFound / 1 PendingReview / 2 ConfirmedDistinct / 3 MergedIntoExisting |
| `distinct_reason` | varchar(500) | NULL | set on ConfirmedDistinct |
| `merged_into_beneficiary_id` | uuid | NULL | FK → beneficiaries (Restrict), indexed |
| `reviewed_at` | timestamptz | NULL | |
| `reviewed_by` | uuid | NULL | reviewer's user id |
| `enrollment_date` | date | NULL (required in practice) | validator gap OQ-18 |
| `registration_facility_id` | uuid | NOT NULL | FK → facilities (Restrict) |
| `area_id` | uuid | NOT NULL | FK → areas (Restrict), indexed |
| `nationality_id` | uuid | NOT NULL | FK → nationalities (Restrict), indexed |
| `is_active` | boolean | NOT NULL | soft delete |

Indexes: unique `(match_key, disambiguator)`; unique `(registration_facility_id, code)`; unique `full_code`; `(normalized_last_name, normalized_first_name)`; `(normalized_last_name, birth_date)`; `(registration_facility_id, created_at)`; `(registration_facility_id, match_review_status)`; `merged_into_beneficiary_id`; `area_id`; `nationality_id` `[BUILT]`.

## beneficiary_identities

| Column | Type | Null | Key / Notes |
|---|---|---|---|
| `beneficiary_id` | uuid | NOT NULL | FK → beneficiaries (Restrict) |
| `identity_type_id` | uuid | NOT NULL | FK → identity_types (Restrict) |
| `value` | varchar(30) | NOT NULL | empty string allowed (`UNREGISTERED`), digit-normalized before save |

Indexes: unique `(beneficiary_id, identity_type_id)` (one document per type); `(identity_type_id, value)` for document lookup `[BUILT]`.

## organizations

| Column | Type | Null | Notes |
|---|---|---|---|
| `ar_name` | varchar(50) | NOT NULL | |
| `en_name` | varchar(50) | NOT NULL | |
| `code` | varchar(10) | NOT NULL | **unique** `ix_organizations_code` |
| `is_active` | boolean | NOT NULL | |

## branches

| Column | Type | Null | Notes |
|---|---|---|---|
| `ar_name` | varchar(50) | NOT NULL | |
| `en_name` | varchar(50) | NOT NULL | |
| `organization_id` | uuid | NOT NULL | FK → organizations — **`NoAction`** (differs from the usual Restrict, OQ-35), indexed |
| `is_active` | boolean | NOT NULL | default true |

⚠ No `code` column despite BR-BRN-01 (OQ-34).

## facilities

| Column | Type | Null | Notes |
|---|---|---|---|
| `ar_name` | varchar(100) | NOT NULL | validator allows 200 (OQ) |
| `en_name` | varchar(100) | NOT NULL | |
| `code` | citext(50) | NOT NULL | **unique** `ix_facilities_code`; generated `{c_code}-{type}-{NNN}` |
| `branch_id` | uuid | NOT NULL | FK → branches (Restrict), indexed |
| `address_id` | uuid | NOT NULL | FK → addresses (Restrict), indexed |
| `type_id` | uuid | NOT NULL | FK → facility_types (Restrict), indexed |
| `partner_id` | uuid | NOT NULL | FK → partners (Restrict) — current partner, indexed |
| `is_active` | boolean | NOT NULL | |

The facility's start date from `AddFacility` is not a `facilities` column — it is persisted as the **initial partner assignment** (`facilities_partners.start_date`) `[BUILT]`.

## facilities_partners (partner history)

| Column | Type | Null | Notes |
|---|---|---|---|
| `facility_id` | uuid | NOT NULL | FK → facilities (Restrict), indexed |
| `partner_id` | uuid | NOT NULL | FK → partners (Restrict), indexed |
| `start_date` | date | NOT NULL | |
| `end_date` | date | NULL | NULL = open/current assignment |
| `project_code` | varchar(50) | NOT NULL | default `''` (BR-FAC-05) |

No `is_active`/no IsActive column — history rows are never deactivated; "current" is `end_date IS NULL` `[BUILT]`.

## facility_types (insert-only lookup)

| Column | Type | Null | Notes |
|---|---|---|---|
| `ar_name` | varchar(100) | NOT NULL | |
| `en_name` | varchar(100) | NOT NULL | |
| `code` | varchar(10) | NOT NULL | **unique**; feeds facility codes |

No `is_active` — rows are never deleted/deactivated `[BUILT]`.

## identity_types

| Column | Type | Null | Notes |
|---|---|---|---|
| `ar_name` | varchar(100) | NOT NULL | |
| `en_name` | varchar(100) | NOT NULL | unique |
| `code` | varchar(30) | NOT NULL | unique — the stable handle (`NATIONAL_ID`, …) |
| `value_regex` | varchar(200) | NOT NULL | validation pattern for document values |

## nationalities, accommodation_types, disability_types, education_types

All four share the shape: `ar_name varchar(100) NOT NULL`, `en_name varchar(100) NOT NULL unique`. No code column. `accommodation`/`disability`/`education` types are not referenced by anything yet (OQ-32).

## partners

| Column | Type | Null | Notes |
|---|---|---|---|
| `ar_name` | varchar(100) | NOT NULL | |
| `en_name` | varchar(100) | NOT NULL | |
| `code` | varchar(10) | NOT NULL | unique |

## addresses (shared)

| Column | Type | Null | Notes |
|---|---|---|---|
| `ar_name` | varchar(200) | NOT NULL | unique per area |
| `en_name` | varchar(200) | NOT NULL | unique per area |
| `area_id` | uuid | NOT NULL | FK → areas (Restrict) |
| `is_active` | boolean | NOT NULL | |

Indexes: unique `(area_id, ar_name)`, unique `(area_id, en_name)` (BR-GEO-03).

## Geography (read-only OCHA lookups)

All rows carry names (varchar(50)) + `code varchar(16)` + audit columns; the hierarchy:

| Table | Columns (beyond audit/names) | Unique constraint |
|---|---|---|
| `countries` | `code` | `code` |
| `cities` | `code`, `c_code varchar(3)`, `country_id NOT NULL FK` | `(country_id, code)`; `c_code` unique |
| `regions` | `code`, `city_id NOT NULL FK` | `(city_id, code)` |
| `districts` | `code`, `region_id NOT NULL FK` | `(region_id, code)` |
| `areas` | `code`, `district_id NOT NULL FK`, `city_id NOT NULL FK` (BR-GEO-02 shortcut) | `(district_id, code)`; index `(city_id, ar_name)` |

All FKs Restrict. Naming note: hierarchy per BR-GEO-01 is Country → City → Region → District → Area; areas also point to their city.

## Planned tables (designed, absent) `[PLANNED]`

`BeneficiaryProfile` (1:1, optional, incl. disabilities + education + accommodation), `WorkingAddresses`, `FacilityBeneficiary` (facility enrollment), `assigned facilities` for ProjectCoordinator, clinics/doctors/medicines/pharmacy transactions (OQ-23, OQ-04, OQ-01).

## Naming inconsistencies to know

1. `FacilityPartnerEntity`, `PartnerEntity` source files are named with a space (`PartnerEntity .cs`) — cosmetic.
2. `branches.organizationId` property is lowercase in C# (compiles; column still `organization_id`).
3. PascalCase identity tables minus snake_case columns — OQ-11.
4. `ApplicationUser` source file is misspelled `AppicationUser.cs`.
5. Two audit columns on `beneficiaries` (`reviewed_at`, `reviewed_by`) that other tables lack — intended for match review.
