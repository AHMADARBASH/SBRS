# SBRS Data Dictionary — Identity and Token Tables
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

Companion to [data-dictionary.md](data-dictionary.md). These tables belong to ASP.NET Core Identity (schema owned by the framework) plus the project-owned `refresh_tokens`.

⚠ **Naming exception**: the Identity tables keep their SQL Server-style PascalCase names (`AspNetUsers`, …) while columns are snake_case — tracked as [OQ-11](../OPEN-QUESTIONS.md#oq-11).

## asp_net_users — table name `AspNetUsers`

| Column | Type | Null | Key / Notes |
|---|---|---|---|
| `id` | uuid | | PK (`pk_asp_net_users`) |
| `user_name` | varchar(256) | NULL | unique via `normalized_user_name` |
| `normalized_user_name` | varchar(256) | NULL | **unique index `UserNameIndex`** |
| `email` / `normalized_email` | varchar(256) | NULL | index `EmailIndex`; login does not use email |
| `email_confirmed` | boolean | NOT NULL | unused (no email flows) |
| `password_hash` | text | NULL | Identity hasher (PBKDF2) |
| `phone_number` | text | NULL | unused today |
| `phone_number_confirmed` | boolean | NOT NULL | |
| `access_failed_count` | integer | NOT NULL | lockout counter |
| `lockout_enabled` | boolean | NOT NULL | ⚠ default false and never set true by the code → lockout not effective (OQ-36) |
| `lockout_end` | timestamptz | NULL | when locked |
| `two_factor_enabled` | boolean | NOT NULL | unused |
| `concurrency_stamp` | text | NULL | |
| `security_stamp` | text | NULL | changed on password/sensitive ops |
| `first_name` | varchar(50) | NOT NULL | project field |
| `last_name` | varchar(50) | NOT NULL | project field |
| `organization_id` | uuid | NULL | FK → organizations (Restrict), indexed |
| `facility_id` | uuid | NULL | FK → facilities (Restrict), indexed |
| `is_active` | boolean | NOT NULL | project field (soft delete of accounts) |
| `is_mobile` | boolean | NOT NULL | project field (mobile users) |
| `created_at` | timestamptz | NOT NULL | project audit |
| `created_by` | uuid | NULL | project audit |

## asp_net_roles — table name `AspNetRoles`

| Column | Type | Null | Notes |
|---|---|---|---|
| `id` | uuid | | PK |
| `name` | varchar(256) | NULL | `SystemAdmin`, `OrganizationAdmin`, … |
| `normalized_name` | varchar(256) | NULL | **unique `RoleNameIndex`** |
| `concurrency_stamp` | text | NULL | |

## link and claim tables (Identity defaults)

| Table | Key | Columns | FK behavior |
|---|---|---|---|
| `AspNetUserRoles` | (`user_id`, `role_id`) PK | — | both Cascade |
| `AspNetUserClaims` | `id` int PK | `claim_type`, `claim_value` | `user_id` Cascade, indexed |
| `AspNetRoleClaims` | `id` int PK | `claim_type`, `claim_value` | `role_id` Cascade, indexed |
| `AspNetUserLogins` | (`login_provider`, `provider_key`) PK | `provider_display_name` | `user_id` Cascade, indexed |
| `AspNetUserTokens` | (`user_id`, `login_provider`, `name`) PK | `value` | `user_id` Cascade |

Only `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` are meaningfully used today (roles + credential validation) `[BUILT]`.

## refresh_tokens (project-owned)

| Column | Type | Null | Key / Notes |
|---|---|---|---|
| `id` | uuid | | PK |
| `user_id` | uuid | NOT NULL | FK → `AspNetUsers` (**Cascade** — the only domain-relevant cascade, alongside Identity's own link tables) |
| `family_id` | uuid | NULL (`BUILT` always set) | all rotations of one login; indexed |
| `token_hash` | varchar(64) | NOT NULL | SHA-256 hex of the raw token; **unique** `ix_refresh_tokens_token_hash` |
| `created_at` | timestamptz | NOT NULL | |
| `expires_at` | timestamptz | NULL | issued with +30 days; "null = never expires" was the design idea but every token sets it `[BUILT]` |
| `used_at` | timestamptz | NULL | burn marker (rotation) |
| `revoked_at` | timestamptz | NULL | logout / family revoke |

Indexes: unique `token_hash`; `family_id`; `user_id` `[BUILT]`.

Lifecycle rules and the reuse-detection flow: [workflows/authentication-and-tokens.md](workflows/authentication-and-tokens.md) and ADR-005. Cleanup of burned/expired rows is `[PLANNED]` (OQ-31).
