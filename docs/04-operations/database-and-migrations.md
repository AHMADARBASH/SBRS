# SBRS Database and Migrations
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

PostgreSQL 14+ with EF Core 10 (Npgsql). Facts verified from the repo `[BUILT]` unless tagged.

## 1. Migration workflow

```bash
# add a migration (one per change, review the generated SQL)
dotnet ef migrations add <Name> -p SBRS.Infrastructure -s SBRS -o Persistence/Migrations

# apply
dotnet ef database update -p SBRS.Infrastructure -s SBRS

# undo the last un-applied migration file
dotnet ef migrations remove -p SBRS.Infrastructure -s SBRS   # NEVER delete files by hand

# generate SQL for review (optional)
dotnet ef migrations script -p SBRS.Infrastructure -s SBRS
```

- Migrations live in `SBRS.Infrastructure/Persistence/Migrations`; the model snapshot (`AppDbContextModelSnapshot.cs`) is the schema source of truth.
- Migrations are applied **manually** — the deploy pipeline does not run `database update` `[BUILT]`. Apply before/as part of each server update.

## 2. Existing migrations (chronology)

| Migration | Date | Content |
|---|---|---|
| `InitialPostgres` | 2026-10-27 | full schema after the SQL Server → PostgreSQL restart (ADR-013), incl. Identity tables + citext |
| `AddFacilityCodeGeneration` | 2026-10-28 | facility code uniqueness/columns (ADR-011) |
| `AddPerFacilityBeneficiaryCode` | 2026-10-28 | beneficiary per-facility `code`, `full_code`, match columns |
| `AddBeneficiaryNationality` | 2026-10-29 | `nationality_id` on beneficiaries |

The database name is capitalised (`SBRS`-style) `[CONFIRMED]`.

## 3. Seed data — current state

| What | Exists? | How |
|---|---|---|
| Roles (6) | ✔ `[BUILT]` | `IdentitySeeder` on every startup |
| First SystemAdmin | ✔ `[BUILT]` (conditional) | `IdentitySeeder`, from secrets `SeedAdmin:*` |
| Geography (OCHA), nationalities, identity types, facility types, partners, accommodation/disability/education types | ✘ `[PLANNED]` (OQ-21) | **no seed scripts exist in the repo** |

Implication: after `database update` the lookup tables are empty; fill them via the SystemAdmin admin APIs or author the intended seed scripts.

## 4. SQL seed scripts (planned set)

None are committed. The intended order when they exist — `geography first` (facility codes depend on city codes):

1. `seed_geography` (OCHA; countries, cities with `c_code`, regions, districts, areas) — **must run before anything else** using codes.
2. `nationalities`
3. `identity_types` (codes match `IdentityTypeCodes.cs` + regexes)
4. `facility_types` (codes used by facility code generation)
5. `partners`
6. `accommodation_types`, `disability_types`, `education_types` (needed for BeneficiaryProfile later)

⚠ Until these scripts exist, do **not** invent facility codes manually — `cities.c_code` drives them.

## 5. Idempotency guidance for future seeds `[ASSUMED]`

- Key lookups on natural codes (`code` unique columns) with `ON CONFLICT (code) DO NOTHING`-style upserts.
- Never touch `AspNet*` rows from SQL (they hold password hashes/concurrency stamps) — the seeder does roles/users.
- Wrap each script in a transaction; log inserted counts.

## 6. PostgreSQL notes carried from the conversion

| Topic | Detail |
|---|---|
| Extensions | `citext` registered by the model; used on `facilities.code`, `beneficiaries.full_code` |
| Naming | domain tables/columns snake_case; **PascalCase identity tables** (`AspNetUsers`, …) with snake_case columns — OQ-11 |
| Keys | identity-by-default columns; `MaxIdentifierLength 63` — keep names ≤ 63 chars |
| Errors | unique violation SQLSTATE **23505** → HTTP 409 (generic retry message); FK violations surface as generic 500 today |
| Booleans | true booleans (`gender` true = male, not smallint) |
| Money/strings | no money usage; texts are `varchar(n)` or `text` |
| Delete behaviour | FKs Restrict (no cascade) except Identity's own link tables + `refresh_tokens.user_id`; `branches.organization_id` = NoAction (OQ-35) |

## 7. Connection string shape

```
Host=<DB_HOST>;Port=5432;Database=SBRS;Username=<DB_USER>;Password=<DB_PASSWORD>
```

Never commit real values (they belong to secrets/env vars; `appsettings.json` ships empty on purpose).
