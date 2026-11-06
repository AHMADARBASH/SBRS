# SBRS Non-Functional Requirements
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

Every requirement carries a status tag; `[BUILT]` = verified in code, `[PLANNED]` = agreed but not implemented, `[ASSUMED]` = proposal, `[OPEN]` = undecided.

## 1. Security

| ID | Requirement | Status | Notes / code location |
|---|---|---|---|
| NFR-SEC-01 | Authentication via JWT bearer tokens; every endpoint (except the anonymous auth trio + swagger) requires a logged-in user through the **fallback policy** | `[BUILT]` | `Policies.Configure` |
| NFR-SEC-02 | Refresh tokens rotate on every use, are single-use, stored SHA-256 hashed, family-revoked on reuse | `[BUILT]` | `RefreshTokenService` (ADR-005) |
| NFR-SEC-03 | Web refresh token delivered only as HttpOnly cookie; access token never persisted client-side | `[BUILT]` (cookie) / `[ASSUMED]` (client storage discipline) | `RefreshCookieService` |
| NFR-SEC-04 | Password policy: ≥ 8 chars with digit, upper, lower. **No symbol requirement today** (OQ-13); default Identity lockout on | `[BUILT]` / `[OPEN]` symbol | `AddInfrastructureDI` |
| NFR-SEC-05 | Role-based authorization with one role per user; policies per module | `[BUILT]` | `Policies.cs` |
| NFR-SEC-06 | Data scoping per role/facility; out-of-scope = 404 | `[BUILT]` | repositories `Scoped()` (ADR-006) |
| NFR-SEC-07 | Deactivation / role change immediately revokes refresh tokens; access token remains valid up to 4 h (accept for now or shorten — OQ-05) | `[BUILT]` / `[OPEN]` lifetime | `UserAdminService`, `RefreshTokenService` |
| NFR-SEC-08 | CORS: explicit origins + credentials for the cookie flow; never `AllowAnyOrigin` with credentials. Current `"*"` default behaves as allow-all-with-credentials — tighten for production (OQ-22) | `[BUILT]` / `[OPEN]` prod config | `Program.cs` |
| NFR-SEC-09 | HTTPS everywhere; HSTS only when configured (ASP.NET defaults) | `[BUILT]` (app.UseHttpsRedirection present) / `[ASSUMED]` reverse proxy TLS | `Program.cs`, deployment docs |
| NFR-SEC-10 | Rate limiting on sensitive search endpoints | `[PLANNED]` | — |

## 2. Privacy and data protection

| ID | Requirement | Status | Notes |
|---|---|---|---|
| NFR-PRV-01 | Identity document values are sensitive: cross-facility search exposes the identity **type code only**, never the value; mother name not exposed either | `[BUILT]` for search DTO | `BeneficiarySearchItemDto` |
| NFR-PRV-02 | A facility sees full demographics only for its own beneficiaries; match review shows what is needed to decide | `[BUILT]` | scoping + `MatchCandidates` |
| NFR-PRV-03 | Request/audit logging must not capture identity values | `[ASSUMED]` — `LoggingBehavior` logs only handler names today; no body logging `[BUILT]` | `LoggingBehavior` |
| NFR-PRV-04 | Searches are audited (who searched for whom, when) | `[PLANNED]` | — |
| NFR-PRV-05 | Reports show aggregated numbers by default; aggregated-only vs individual records is undecided (OQ-03); reports must count **distinct beneficiaries** separately from services | `[OPEN]` / `[PLANNED]` | UC-14 |
| NFR-PRV-06 | Audit columns on every record (created/updated by/at) give a change trail; a dedicated audit log exists `[OPEN]` | `[BUILT]` columns | `AppDbContext` audit stamping |
| NFR-PRV-07 | Data residency / retention rules for Syria context | `[OPEN]` | — |

## 3. Performance and scalability

| ID | Requirement | Status | Notes |
|---|---|---|---|
| NFR-PER-01 | Beneficiary roster is paginated — fixed page size 30 — and never loads unbounded lists | `[BUILT]` | `PagedResult<T>` |
| NFR-PER-02 | Cross-facility search capped at 20 results with `hasMore` | `[BUILT]` | search handler |
| NFR-PER-03 | Name-search indexes: `(normalized_last, normalized_first)`, `(normalized_last, birth_date)`, `(facility, review_status)` etc. | `[BUILT]` | `BeneficiaryConfiguration` indexes |
| NFR-PER-04 | Registration concurrency: per-facility row lock (`FOR UPDATE`) keeps codes gap-free without a counter table | `[BUILT]` | `BeneficiaryRepository` |
| NFR-PER-05 | Target latency/response-time SLOs for search under load | `[OPEN]` — no numbers agreed | — |
| NFR-PER-06 | MaxIdentifierLength 63 (PostgreSQL limit), identity-by-default columns — schema uses PostgreSQL-native identity keys | `[BUILT]` | model snapshot annotations |

## 4. Localisation (Arabic-first)

| ID | Requirement | Status | Notes |
|---|---|---|---|
| NFR-LOC-01 | All names/templates carry Arabic + English columns; Arabic is the working language | `[CONFIRMED]` `[BUILT]` | every lookup `arName`/`enName` |
| NFR-LOC-02 | Matching normalises Arabic: trim, diacritics + tatweel removal, alef unification, ى→ي, ة→ه, Arabic/Persian digits → ASCII | `[BUILT]` | `ArabicTextNormalizer`, `IdentityValueNormalizer` |
| NFR-LOC-03 | English UI toggle for clients | `[OPEN]` (OQ-09) | — |
| NFR-LOC-04 | RTL layout support in the clients | `[ASSUMED]` frontend requirement | — |

## 5. Availability, backup, operations

| ID | Requirement | Status | Notes |
|---|---|---|---|
| NFR-AVL-01 | Single Linux server with systemd + PostgreSQL; restart procedure documented | `[BUILT]` pipeline / `[ASSUMED]` ops details | [../04-operations/deployment-linux.md](../04-operations/deployment-linux.md) |
| NFR-AVL-02 | Regular `pg_dump` backups with tested restore | `[PLANNED]` — no scripts in repo | [../04-operations/backup-and-restore.md](../04-operations/backup-and-restore.md) |
| NFR-AVL-03 | Structured application logs (console) with correlation; Serilog/AppInsights not used | `[BUILT]` default logging / `[OPEN]` sinks | — |
| NFR-AVL-04 | Health-check endpoint for monitoring | `[OPEN]` — none in code | — |
| NFR-AVL-05 | Database migrations applied via `dotnet ef database update` (manual; not in pipeline) | `[CONFIRMED]` | [../04-operations/database-and-migrations.md](../04-operations/database-and-migrations.md) |

## 6. Maintainability

| ID | Requirement | Status | Notes |
|---|---|---|---|
| NFR-MNT-01 | Clean architecture layering with one-way dependencies | `[BUILT]` | [../02-design/architecture.md](../02-design/architecture.md) (ADR-001) |
| NFR-MNT-02 | One file per operation (command/validator/handler), no AutoMapper | `[BUILT]` | manual `ToDto` mappings |
| NFR-MNT-03 | Test suite covering scoping, policies, token rotation, code generation, matching | `[OPEN]` — **no test projects exist** | [../05-testing/test-strategy.md](../05-testing/test-strategy.md) |
| NFR-MNT-04 | Migrations only via EF tooling (`migrations remove`, never hand-delete) | `[CONFIRMED]` | conventions in ARCHITECTURE.md |
| NFR-MNT-05 | API contract documentation via OpenAPI (Swagger) available in all environments | `[BUILT]` (enabled in prod too — OQ-25) | `Program.cs` |
