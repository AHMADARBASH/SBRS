# SBRS Business Rules
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

One table per area. `Status` is one of `[CONFIRMED]`, `[ASSUMED]`, `[OPEN]`, `[PLANNED]`, `[BUILT]` (code verified — for planned rules the *intent* may still be `[CONFIRMED]`; the tag reflects the rule's implementation status). Enforced-in points at the exact code location.

Cross-references: use cases [use-cases/README.md](use-cases/README.md), endpoints [../03-integration/endpoint-reference.md](../03-integration/endpoint-reference.md), decisions [../02-design/decisions/README.md](../02-design/decisions/README.md).

## 1. General (BR-GEN)

| ID | Rule | Status | Enforced in |
|---|---|---|---|
| BR-GEN-01 | Every entity has `Id` (Guid), `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy`; `AppDbContext` stamps them on save | `[CONFIRMED]` `[BUILT]` | `BaseEntity`, `AppDbContext.SaveChangesAsync` override |
| BR-GEN-02 | Records are never hard-deleted; deactivation sets `IsActive = false`. FacilityTypes are never deleted (insert-only, no IsActive) | `[CONFIRMED]` `[BUILT]` | `*.Deactivate()` entity methods; no `Remove()` calls in repositories |
| BR-GEN-03 | Parent ids (OrganizationId on branch, BranchId on facility, AreaId on address, …) are immutable after creation; only Update endpoints for organization/branch/facility/address accept non-parent fields | `[CONFIRMED]` `[BUILT]` | `UpdateBranchRequest`/`UpdateFacilityRequest`/`UpdateAddressRequest` contracts + `*Entity.Update` signatures |
| BR-GEN-04 | Codes are trimmed before saving | `[CONFIRMED]` `[BUILT]` | handlers (`c.Code!.Trim()`), `FacilityCodeBuilder`, `BeneficiaryFullCodeBuilder`, `IdentityValuePatterns.Validate` |
| BR-GEN-05 | Records outside the caller's scope are returned as **404**, not 403 | `[CONFIRMED]` `[BUILT]` | `Scoped()` filters in repositories; `NotFoundException` in handlers (ADR-006) |
| BR-GEN-06 | Foreign keys use restrict (no cascade delete) | `[CONFIRMED]` `[BUILT]` | EF configurations — `DeleteBehavior.Restrict` (ClientSetNull for optional FKs) |
| BR-GEN-07 | All unique checks race on the DB: a PostgreSQL `23505` unique violation is mapped to HTTP 409 with a generic "created by someone else" message | `[CONFIRMED]` `[BUILT]` | `GlobalExceptionHandler` (NpgsqlException constraint check) |
| BR-GEN-08 | Activation/reactivation endpoints do not exist; deactivation is one-way through the API | `[ASSUMED]` `[BUILT]` (no activate route in any controller) | controllers |

## 2. Organizations (BR-ORG)

| ID | Rule | Status | Enforced in |
|---|---|---|---|
| BR-ORG-01 | Organization code is unique system-wide | `[CONFIRMED]` `[BUILT]` | unique index `ix_organizations_code` + `IOrganizationRepository.CodeExistsAsync` + DB race → 409 |
| BR-ORG-02 | Only SystemAdmin adds organizations; SystemAdmin and OrganizationAdmin update (OA only their own — enforced by scoped fetch, not by policy) | `[CONFIRMED]` `[BUILT]` | `Policies.SystemAdminOnly` on Add; `OrganizationLevel` on Update; `OrganizationRepository.Scoped()` |
| BR-ORG-03 | A deactivated organization cannot be updated, and cannot be deactivated twice | `[CONFIRMED]` `[BUILT]` | `OrganizationEntity.Update/Deactivate` (DomainException) |
| BR-ORG-04 | The org deactivate endpoint exists in code but is commented out in the controller | `[BUILT]` ⚠ | `OrganizationsController` (see OQ-15) |

## 3. Branches (BR-BRN)

| ID | Rule | Status | Enforced in |
|---|---|---|---|
| BR-BRN-01 | Branch code is unique per organization | `[CONFIRMED]` `[BUILT]` | composite unique index `(organization_id, code)` in `BranchConfiguration` |
| BR-BRN-02 | The HQ is a branch, not a separate entity | `[CONFIRMED]` | data-entry convention; no HQ type in code |
| BR-BRN-03 | A branch cannot be deactivated while it has active facilities | `[ASSUMED]` `[BUILT]` (guard implemented) ⚠ but **no deactivate endpoint exists** | `IBranchRepository.HasActiveFacilitiesAsync` — never called (OQ-16) |
| BR-BRN-04 | A deactivated branch cannot be updated, and cannot be deactivated twice | `[CONFIRMED]` `[BUILT]` | `BranchEntity.Update/Deactivate` |
| BR-BRN-05 | A branch belongs to exactly one organization; `organizationId` cannot change after creation | `[CONFIRMED]` `[BUILT]` | `AddBranchRequest` requires it; `UpdateBranchRequest` lacks it |

## 4. Facilities (BR-FAC)

| ID | Rule | Status | Enforced in |
|---|---|---|---|
| BR-FAC-01 | A facility belongs to one branch, has one required address and one required facility type | `[CONFIRMED]` `[BUILT]` | `AddFacilityCommand` (all NotEmpty) + `FacilityConfiguration` required FKs |
| BR-FAC-02 | Facility code format `{city c_code}-{facility type code}-{3-digit sequence}`; sequence = (count of facilities of that city) + 1, no counter column. City c_codes (DAM, ALP, RUD, …) live in `cities` seed `[OPEN: seed missing, OQ-21]` | `[CONFIRMED]` `[BUILT]` | `FacilityCodeBuilder`, `AddFacilityCommandHandler` (query count + insert) |
| BR-FAC-03 | Facility code is unique system-wide (`facilities.code` is `citext`, max 50, unique index) | `[CONFIRMED]` `[BUILT]` | unique index `ix_facilities_code`; unique-violation → 409 |
| BR-FAC-04 | A facility has a current partner (`PartnerId`) plus full history in `facilities_partners` (StartDate, EndDate, ProjectCode); assigning a new partner closes the old assignment with `EndDate = StartDate - 1 day` | `[CONFIRMED]` `[BUILT]` | `FacilityPartnerDomain.Assign/Close`, `AssignFacilityPartnerCommandHandler` |
| BR-FAC-05 | `ProjectCode` is optional in the API but stored NOT NULL with an empty-string default (max 50) | `[CONFIRMED]` `[BUILT]` | `FacilityPartnerConfiguration.HasDefaultValue("")` |
| BR-FAC-06 | Add/update facilities: SystemAdmin and OrganizationAdmin (scope: SA all, OA by the facility's organization). **BR-FAC-06 intended FA/DE "own facility" rights, but code restricts to OrganizationLevel** | `[CONFIRMED]` intent / `[BUILT]` code mismatch | `Policies.OrganizationLevel` on controller (OQ-33) |
| BR-FAC-07 | A facility cannot be added to a deactivated branch; deactivated facilities cannot be updated, get a new partner, or be deactivated twice | `[CONFIRMED]` `[BUILT]` | `BranchEntity`/`FacilityEntity` guard methods (DomainException → 400) |
| BR-FAC-08 | Facility start date is required (assignment of the facility's operations), stored as `date` | `[CONFIRMED]` `[BUILT]` | `AddFacilityCommand.StartDate` NotEmpty + `facilities.start_date` |
| BR-FAC-09 | A new partner assignment must start later than the currently open assignment's start date | `[CONFIRMED]` `[BUILT]` | `FacilityPartnerDomain.Assign` (DomainException) |
| BR-FAC-10 | Partner end date cannot precede its start date | `[CONFIRMED]` `[BUILT]` | `FacilityPartnerDomain.Close` restriction |

## 5. Geography and addresses (BR-GEO)

| ID | Rule | Status | Enforced in |
|---|---|---|---|
| BR-GEO-01 | Hierarchy Country → City → Region → District → Area; read-only lookups (no write endpoints) seeded from OCHA Syria admin boundaries + populated places; `Code` is the OCHA pcode, unique within its parent | `[CONFIRMED]` [BUILT hierarchy/FKs] / `[OPEN]` seeding (OQ-21) | `GeographyConfiguration`, `api/Geography` GET-only controller |
| BR-GEO-02 | Areas also carry `CityId` so a client can fetch all areas of a selected city | `[CONFIRMED]` `[BUILT]` | `AreaEntity.CityId`, `GET api/Geography/cities/{cityId}/areas` |
| BR-GEO-03 | Addresses are unique per (AreaId, ArName) and per (AreaId, EnName); no Code; shared system-wide data readable by every logged-in user | `[CONFIRMED]` `[BUILT]` | composite unique indexes in `AddressConfiguration`; read endpoints under fallback policy |
| BR-GEO-04 | An address cannot be deactivated while an active facility uses it | `[CONFIRMED]` ⚠ guard **not implemented**, no deactivate endpoint | — (OQ-17) |
| BR-GEO-05 | Mobile users choose a working location (address) saved per facility (`WorkingAddresses`) | `[PLANNED]` | — |
| BR-GEO-06 | Address names are mutable (Update) but `AreaId` is immutable | `[CONFIRMED]` `[BUILT]` | `UpdateAddressRequest` has no areaId; `AddressEntity.Update` |

## 6. Users and authentication (BR-USR)

| ID | Rule | Status | Enforced in |
|---|---|---|---|
| BR-USR-01 | Exactly one role per user. Roles: SystemAdmin, OrganizationAdmin, BranchAdmin, ProjectCoordinator, FacilityAdmin, DataEntry | `[CONFIRMED]` `[BUILT]` | `Roles.All`, `UserAdminService` (single `AddToRoleAsync`) |
| BR-USR-02 | Only SystemAdmin creates, updates, deactivates users and resets passwords | `[CONFIRMED]` `[BUILT]` | controller-level `Policies.SystemAdminOnly` on Users |
| BR-USR-03 | Scope: OrganizationAdmin needs OrganizationId; BranchAdmin needs BranchId `[PLANNED]`; FA/DE need exactly one FacilityId; ProjectCoordinator ≥ 1 assigned facility `[PLANNED]`. OrganizationId is derived by the system, never hand-picked | `[CONFIRMED]` rule `[BUILT]` for OA/FA/DE in `UserAdminService.ResolveScopeAsync`; BA/PC creation rejected with "cannot be managed yet" | `AddUserCommandValidator` + `UserAdminService` (OQ-04, OQ-28) |
| BR-USR-04 | Login by username only (no email). Password ≥ 8 chars with digit + upper + lower (`RequireSymbol` is **not** enabled — OQ-13). Default lockout enabled | `[CONFIRMED]` rule `[BUILT]` partial (no symbol requirement) | `AddInfrastructureDI` Identity options; Identity lockout defaults |
| BR-USR-05 | Inactive, locked-out or role-less users cannot log in or refresh | `[CONFIRMED]` `[BUILT]` | `IdentityService.ValidateCredentialsAsync`, `RefreshTokenService.RotateAsync` (user reloaded) |
| BR-USR-06 | Deactivating a user revokes their refresh tokens | `[CONFIRMED]` `[BUILT]` | `DeactivateUserCommandHandler` → `IRefreshTokenService.RevokeAllForUserAsync` |
| BR-USR-07 | `IsMobile` marks users who work on the mobile app; carried as the `is_mobile` claim | `[CONFIRMED]` `[BUILT]` | `TokenService.CreateToken`, `ApplicationUser.IsMobile` |
| BR-USR-08 | A user cannot deactivate their own account or change their own role | `[CONFIRMED]` `[BUILT]` | `UserAdminService` (self checks) |
| BR-USR-09 | ⚠ Deactivation currently checks the **caller's** role instead of the target's, so SystemAdmin can never deactivate anyone (probable bug) | `[BUILT]` ⚠ | `DeactivateUserCommandHandler` (OQ-14) |
| BR-USR-10 | Username rules: 3–50 chars, only letters/digits/`.`/`_`/`-`; unique (409 on duplicate) | `[CONFIRMED]` `[BUILT]` | `AddUserCommandValidator`, unique index on `AspNetUsers.NormalizedUserName` |

## 7. Beneficiaries (BR-BEN)

| ID | Rule | Status | Enforced in |
|---|---|---|---|
| BR-BEN-01 | A beneficiary is a person registered **once**, unique across all facilities and branches | `[CONFIRMED]` `[BUILT]` (via match pipeline; DB uniqueness only for identity + full_code) | `RegisterBeneficiaryCommandHandler` |
| BR-BEN-02 | Duplicate key = five fields: first, father, last, mother (her first name only) names + birth date, all normalized. Exact = SHA-256 MatchKey; probable = weighted score (first 25, father 15, last 20, mother 25, birth 15; ≥ 70 = probable; year-only half weight; twin rule always reviewed). Weights/thresholds need real-data testing | `[CONFIRMED]` key `[ASSUMED]` scoring `[BUILT]` | `BeneficiaryMatchKey`, `BeneficiaryMatchScorer`, `ArabicTextNormalizer` |
| BR-BEN-03 | Naming convention: common use "first – last"; official documents "first – father – last", then mother | `[CONFIRMED]` | data convention (search uses first+last) |
| BR-BEN-04 | Gender is a bit: 1 = male, 0 = female | `[CONFIRMED]` `[BUILT]` | `BeneficiaryEntity.Gender` (bool) rendered as `gender` in DTOs |
| BR-BEN-05 | If only the birth year is known, store 1 January of that year and set `IsBirthDateEstimated = true` | `[CONFIRMED]` | **entry-time rule** — the client must send `YYYY-01-01` (no server auto-conversion `[BUILT]`); matching gives half weight on year equality `[BUILT]` |
| BR-BEN-06 | `Code` = per-facility sequence with a `SELECT … FOR UPDATE` lock on the facility row so concurrent registrations never collide; `FullCode = {facility code}-{code}` (no padding), unique system-wide | `[CONFIRMED]` `[BUILT]` | `BeneficiaryRepository` (`SELECT 1 FROM facilities … FOR UPDATE` in `IBeneficiaryRepository` contract), `BeneficiaryFullCodeBuilder` |
| BR-BEN-07 | `AreaId` is required at registration; the request carries AreaId only — no city is stored or derived (OQ-27) | `[CONFIRMED]` required `[ASSUMED]` "derived" → **not derived** | `RegisterBeneficiaryCommandValidator`, `BeneficiaryEntity` (no CityId) |
| BR-BEN-08 | `EnrollmentDate` is required in the register request (not in the future). ⚠ Validator misses default `0001-01-01` | `[CONFIRMED]` rule / `[BUILT]` partial | `RegisterBeneficiaryCommandValidator` (OQ-18) |
| BR-BEN-09 | Registration requires ≥ 1 identity saved to `beneficiary_identities`; value must match the type regex; UNREGISTERED must have empty value; one identity per type per beneficiary; value trimmed + Arabic/Persian digits → ASCII before regex | `[CONFIRMED]` `[BUILT]` | `BeneficiaryIdentityEntity`, `IdentityValuePatterns`, `IdentityValueNormalizer`, unique index (beneficiary, type) |
| BR-BEN-10 | Cross-facility search by FullCode **or** first+last name (starts-with on normalized names, ≥ 2 chars each, ≤ 20 results, `hasMore` flag); inactive records excluded; merged duplicates hidden; merged FullCode resolves to the survivor | `[CONFIRMED]` inputs `[ASSUMED]` limits `[BUILT]` | `SearchBeneficiariesQueryHandler`, `BeneficiaryRepository` (OQ-12 note: SA/DE/FA only) |
| BR-BEN-11 | Search results show fullCode, first/father/last name, birth date (+estimated), gender, identity **type code** — never the identity value or mother name | `[ASSUMED]` `[BUILT]` | `BeneficiarySearchItemDto` |
| BR-BEN-12 | Match cases go to the FacilityAdmin: ConfirmSame merges into an existing (candidate) beneficiary — the new record becomes `MergedIntoExisting` and is hidden; ConfirmNotAMatch sets `ConfirmedDistinct` (+ optional reason ≤ 500); only PendingReview records are reviewable | `[CONFIRMED]` `[BUILT]` | `ConfirmBeneficiarySameCommandHandler` (candidate check), `ConfirmBeneficiaryNotAMatchCommandHandler`, `BeneficiaryMatchReviewStatus` |
| BR-BEN-13 | DataEntry searches+registers; FacilityAdmin searches+reviews; SystemAdmin both (see OQ-12: register is DE-only in code). Policies: `RegisterBeneficiaries`, `SearchBeneficiaries`, `ReviewBeneficiaryMatches` | `[CONFIRMED]` / `[BUILT]` with deviation | `Policies.cs` |
| BR-BEN-14 | GetAll returns the caller's facility beneficiaries, paginated (**30/page**), newest first, active only; SystemAdmin must pass `facilityId`. "Within the facility" = `RegistrationFacilityId` until `FacilityBeneficiary` exists | `[ASSUMED]` `[BUILT]` | `GetBeneficiariesQueryHandler`, `PagedResult<T>` |
| BR-BEN-15 | EducationIssue (reason for no education) is a lookup applying only when the person has no education | `[OPEN]` — nothing built | — (OQ-07) |
| BR-BEN-16 | Beneficiary core + optional 1:1 `BeneficiaryProfile` are separate tables; `IsDisabled` derives from beneficiary-disabilities rows, not stored | `[CONFIRMED]` `[PLANNED]` | — (OQ-23) |
| BR-BEN-17 | Request logging must never log identity values | `[ASSUMED]` `[PLANNED]` | `LoggingBehavior` currently logs only names, not body content `[BUILT]` |
| BR-BEN-18 | Exact-key duplicates under review are allowed: `Disambiguator` (0, 1, 2…) + unique index `(match_key, disambiguator)`; a new record with a duplicate key starts as `PendingReview` | `[CONFIRMED]` `[BUILT]` | `BeneficiaryConfiguration`, `RegisterBeneficiaryCommandHandler` |
| BR-BEN-19 | An identity document (type + non-empty value ≠ UNREGISTERED) that matches an existing record is an **Exact** match (score 100, reason `SameIdentity` = 256) regardless of names | `[CONFIRMED]` `[BUILT]` | `BeneficiaryMatchScorer` + identity probe in registration |
| BR-BEN-20 | Deactivation of a beneficiary is a soft delete (DELETE route) permitted to SA/FA/DE; a deactivated person must not be accidentally re-registered — match search includes inactive records | `[CONFIRMED]` `[BUILT]` | `DeactivateBeneficiary`-style handler, `GetProbableCandidatesAsync` (inactive included on purpose) |

## 8. Authentication/token rules (BR-AUTH)

| ID | Rule | Status | Enforced in |
|---|---|---|---|
| BR-AUTH-01 | Access token: JWT, 240 minutes, claims sub/unique_name/role/jti/organization_id/facility_id (+ is_mobile), signed HS256 from `Jwt:Key` | `[CONFIRMED]` `[BUILT]` | `TokenService`, `JwtSettings` |
| BR-AUTH-02 | Refresh token: 30 days, SHA-256 hashed at rest, single-use, rotating, family-scoped; reuse of a burned token revokes the family | `[CONFIRMED]` `[BUILT]` | `RefreshTokenService`, `RefreshTokenConfiguration` |
| BR-AUTH-03 | Web clients receive the refresh token as HttpOnly cookie `sbrs_rt` when sending `X-Client-Type: web`; the body then carries null | `[CONFIRMED]` `[BUILT]` | `AuthController` + `RefreshCookieService` |
| BR-AUTH-04 | All four token endpoints (login/refresh/logout) are anonymous and take the refresh token from the body or cookie; `logout-all` and `me` require authentication | `[CONFIRMED]` `[BUILT]` | `AuthController` attributes |
| BR-AUTH-05 | Known caveat: an access token stays valid until expiry after logout/deactivation/role change (up to 4 h) | `[CONFIRMED]` `[BUILT]` | no per-request session check (OQ-05) |
