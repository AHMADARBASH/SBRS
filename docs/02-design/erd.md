# SBRS Entity-Relationship Diagram
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

Generated from the EF model snapshot (`AppDbContextModelSnapshot.cs`), not from memory. A human-drawn version will live as `erd.png` next to this file (added by the owner):

![SBRS ERD (owner's diagram)](erd.png)

## 1. Mermaid ERD — geography and addresses

```mermaid
erDiagram
    COUNTRIES ||--o{ CITIES : "country_id"
    CITIES ||--o{ REGIONS : "city_id"
    REGIONS ||--o{ DISTRICTS : "region_id"
    DISTRICTS ||--o{ AREAS : "district_id"
    CITIES |o--o{ AREAS : "city_id (shortcut)"
    AREAS ||--o{ ADDRESSES : "area_id"

    COUNTRIES {
        uuid id PK
        varchar ar_name
        varchar en_name
        varchar code "unique (pcode)"
    }
    CITIES {
        uuid id PK
        varchar ar_name
        varchar en_name
        varchar code
        varchar c_code "unique, facility code prefix"
    }
    REGIONS { uuid id PK varchar code }
    DISTRICTS { uuid id PK varchar code }
    AREAS {
        uuid id PK
        varchar code "unique per district"
        uuid city_id FK
        uuid district_id FK
    }
    ADDRESSES {
        uuid id PK
        uuid area_id FK
        varchar ar_name "uniq per area"
        varchar en_name "uniq per area"
        boolean is_active
    }
```

## 2. Organization structure and partners

```mermaid
erDiagram
    ORGANIZATIONS ||--o{ BRANCHES : "organization_id"
    BRANCHES ||--o{ FACILITIES : "branch_id"
    AREAS ||--o{ FACILITIES : "address_id"
    FACILITY_TYPES ||--o{ FACILITIES : "type_id"
    PARTNERS ||--o{ FACILITIES : "partner_id (current)"
    PARTNERS ||--o{ FACILITIES_PARTNERS : "partner_id"
    FACILITIES ||--o{ FACILITIES_PARTNERS : "facility_id"
    ORGANIZATIONS |o--o{ ASPNET_USERS : "organization_id"
    FACILITIES |o--o{ ASPNET_USERS : "facility_id"

    ORGANIZATIONS {
        uuid id PK
        varchar code "unique, 10"
        varchar ar_name
        varchar en_name
        boolean is_active
    }
    BRANCHES {
        uuid id PK
        uuid organization_id FK
        varchar ar_name
        varchar en_name
        boolean is_active
    }
    FACILITIES {
        uuid id PK
        uuid branch_id FK
        uuid address_id FK
        uuid type_id FK
        uuid partner_id FK
        citext code "unique, generated"
        varchar ar_name "100"
        varchar en_name "100"
        boolean is_active
    }
    FACILITY_TYPES {
        uuid id PK
        varchar code "unique, 10"
        varchar ar_name
        varchar en_name
    }
    PARTNERS {
        uuid id PK
        varchar code "unique, 10"
        varchar ar_name
        varchar en_name
    }
    FACILITIES_PARTNERS {
        uuid id PK
        uuid facility_id FK
        uuid partner_id FK
        date start_date
        date end_date "null = open"
        varchar project_code "not null, default ''"
    }
```

## 3. Beneficiaries

```mermaid
erDiagram
    FACILITIES ||--o{ BENEFICIARIES : "registration_facility_id"
    AREAS ||--o{ BENEFICIARIES : "area_id"
    NATIONALITIES |o--o{ BENEFICIARIES : "nationality_id"
    BENEFICIARIES ||--o{ BENEFICIARY_IDENTITIES : "beneficiary_id"
    IDENTITY_TYPES ||--o{ BENEFICIARY_IDENTITIES : "identity_type_id"
    BENEFICIARIES |o--o{ BENEFICIARIES : "merged_into_beneficiary_id"

    BENEFICIARIES {
        uuid id PK
        citext full_code "unique"
        int code "per-facility"
        int disambiguator "same key twin index"
        varchar match_key "char(64), sha256"
        smallint match_review_status
        uuid merged_into_beneficiary_id FK
        varchar distinct_reason "500"
        timestamp reviewed_at
        uuid reviewed_by FK
        varchar first_name
        varchar father_name
        varchar last_name
        varchar mother_name
        varchar normalized_first_name
        varchar normalized_father_name
        varchar normalized_last_name
        varchar normalized_mother_name
        date birth_date
        boolean is_birth_date_estimated
        boolean gender "true=male"
        date enrollment_date
        uuid registration_facility_id FK
        uuid area_id FK
        uuid nationality_id FK
        boolean is_active
    }
    BENEFICIARY_IDENTITIES {
        uuid id PK
        uuid beneficiary_id FK
        uuid identity_type_id FK
        varchar value "30, normalized"
    }
    IDENTITY_TYPES {
        uuid id PK
        varchar code "unique, 30"
        varchar ar_name
        varchar en_name
        varchar value_regex "200"
    }
    NATIONALITIES {
        uuid id PK
        varchar ar_name
        varchar en_name "unique"
    }
```

## 4. Identity, users, tokens

```mermaid
erDiagram
    ASPNET_USERS ||--o{ ASPNET_USER_ROLES : "user_id"
    ASPNET_ROLES ||--o{ ASPNET_USER_ROLES : "role_id"
    ASPNET_USERS ||--o{ ASPNET_USER_CLAIMS : "user_id"
    ASPNET_USERS ||--o{ ASPNET_USER_LOGINS : "user_id"
    ASPNET_USERS ||--o{ ASPNET_USER_TOKENS : "user_id"
    ASPNET_ROLES ||--o{ ASPNET_ROLE_CLAIMS : "role_id"
    ASPNET_USERS ||--o{ REFRESH_TOKENS : "user_id (cascade)"

    ASPNET_USERS {
        uuid id PK
        varchar user_name "unique normalized"
        varchar first_name "50"
        varchar last_name "50"
        uuid organization_id FK
        uuid facility_id FK
        boolean is_active
        boolean is_mobile
        int access_failed_count
        timestamp lockout_end
    }
    ASPNET_ROLES {
        uuid id PK
        varchar name
        varchar normalized_name "unique"
    }
    REFRESH_TOKENS {
        uuid id PK
        uuid user_id FK
        uuid family_id
        varchar token_hash "unique, sha256"
        timestamp created_at
        timestamp expires_at "30 days"
        timestamp used_at "rotation"
        timestamp revoked_at
    }
```

## 5. Simple lookup tables

`accommodation_types`, `disability_types`, `education_types` — `id`, `ar_name(100)`, `en_name(100) unique`, audit columns. Currently referenced by nothing (OQ-32).

## 6. Cardinality notes

- All geometry/organization FKs are **required** (non-null `Guid` + domain checks); optional ones are `beneficiaries.merged_into_beneficiary_id`, `beneficiaries.reviewed_by`, `aspnet_users.organization_id/facility_id`, `areas.city_id`? — **no**: `areas.city_id` and `district_id` are required by the entity; the parser showed them nullable only because nullability came from configuration. Entity types say non-null `[BUILT]`.
- All domain FKs are **Restrict** (no cascade); the Identity-owned tables (`AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims`) and `refresh_tokens.user_id` are **Cascade** `[BUILT]` (BR-GEN-06; `branches.organization_id` = NoAction, OQ-35).
- `beneficiaries.merged_into_beneficiary_id` is the merge link; the merged row keeps its own row (full history) and is hidden by `match_review_status = 3`.
- `facilities.partner_id` is required at creation (domain check "Partner is required."); `facilities_partners.end_date` NULL marks the open assignment.
