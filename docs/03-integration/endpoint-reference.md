# SBRS Endpoint Reference
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

Generated from the controllers and contracts (not from memory). Routes are documented **exactly as implemented** — the style is inconsistent (action-style `Add`, `Update/{id}`, `GetAll` vs REST-ish `countries/{id}/cities`); tracked in [OPEN-QUESTIONS.md](../OPEN-QUESTIONS.md#oq-06). Every route needs the bearer token unless marked **anonymous** (Auth login/refresh/logout).

Roles: **SA** SystemAdmin · **OA** OrganizationAdmin · **BA** BranchAdmin · **PC** ProjectCoordinator · **FA** FacilityAdmin · **DE** DataEntry ("logged-in" = any authenticated user).

## 1. Auth — `api/Auth` (`AuthController`)

| Method | Route | Roles | Request body | Success | Response | Errors |
|---|---|---|---|---|---|---|
| POST | `api/Auth/login` | **anonymous** | `userName` (req), `password` (req) | 200 | `LoginResultDto` | 400 (validation), 401 (bad credentials / inactive / locked / no role) |
| GET | `api/Auth/me` | logged-in | — | 200 | `{ userId, role, organizationId, facilityId, isSystemAdmin }` | |
| POST | `api/Auth/refresh` | **anonymous** (web: token from HttpOnly cookie) | `refreshToken` (mobile req; web ignored) | 200 | `LoginResultDto` (`refreshToken` null for web) | 400 (mobile empty), 401 |
| POST | `api/Auth/logout` | **anonymous** | `refreshToken`(mobile req; web ignored) | 204 | — | 400 (mobile empty) |
| POST | `api/Auth/logout-all` | logged-in | — | 204 | — | |

## 2. Users — `api/Users` (controller-level **SA only**)

| Method | Route | Request body | Success | Response | Errors |
|---|---|---|---|---|---|
| GET | `api/Users/Roles` | — | 200 | `[{ role, requiresOrganizationId, requiresFacilityId }]` | |
| POST | `api/Users/CreateUser` | `userName` (3–50, `[A-Za-z0-9._-]`), `password` (≥8), `firstName`, `lastName`, `role` (Roles constant), `organizationId?` (OrgAdmin req), `facilityId?` (FA/DE req), `isMobile` (bool) | 201 | `UserDto` | 400 (validation / role-scope rules / inactive org / facility), 409 (username taken) |
| PUT | `api/Users/UpdateUser` | `id`, `firstName`, `lastName`, `role`, `organizationId?`, `facilityId?`, `isMobile` | 200 | `UserDto` | 400, 404 |
| POST | `api/Users/deactivateUser/{id}` | — | 204 | — | 400 (self-account; see [OQ-14](../OPEN-QUESTIONS.md#oq-14) bug), 404 |
| POST | `api/Users/reset-password` | `id`, `newPassword` (≥8) | 204 | — | 400, 404 |

No user list/read endpoints exist yet ([OQ-10](../OPEN-QUESTIONS.md#oq-10)). Role change or scope change revokes the user's refresh tokens.

## 3. Organizations — `api/organizations`

| Method | Route | Roles | Request | Success | Response | Errors |
|---|---|---|---|---|---|---|
| GET | `api/organizations/GetAll?includeInactive=` | logged-in | query | 200 | `OrganizationDto[]` (scoped: see [roles-and-permissions](../01-analysis/roles-and-permissions.md)) | |
| GET | `api/organizations/GetById/{id}` | logged-in | — | 200 | `OrganizationDto` | 404 |
| POST | `api/organizations/Add` | SA | `arName`, `enName`, `code` (all req, ≤50/50/10) | 201 | `OrganizationDto` | 400, 409 (code exists) |
| PUT | `api/organizations/Update/{id}` | SA, OA | same (OA implicitly limited to own org by repository scoping) | 200 | `OrganizationDto` | 400, 404, 409 |

Deactivate endpoint is commented out ([OQ-15](../OPEN-QUESTIONS.md#oq-15)).

## 4. Branches — `api/Branches`

| Method | Route | Roles | Request | Success | Response | Errors |
|---|---|---|---|---|---|---|
| GET | `api/Branches/GetAll?includeInactive=` | logged-in (scoped) | query | 200 | `BranchDto[]` | |
| GET | `api/Branches/GetById/{id}` | logged-in (scoped) | — | 200 | `BranchDto[]` (single-element list; [OQ-26](../OPEN-QUESTIONS.md#oq-26)) | 404 |
| GET | `api/Branches/GetByOrganizationId/{organizationId}` | logged-in (scoped) | — | 200 | `BranchDto[]` | |
| POST | `api/Branches/Add` | SA, OA | `organizationId` (req), `arName`, `enName` (≤50) | 201 | `BranchDto` | 400, 404 (org) |
| PUT | `api/Branches/Update/{id}` | SA, OA | `arName`, `enName` | 200 | `BranchDto` | 400, 404 |

## 5. Facilities — `api/Facilities`

| Method | Route | Roles | Request | Success | Response | Errors |
|---|---|---|---|---|---|---|
| GET | `api/Facilities/GetByBranchId/{branchId}?includeInactive=` | logged-in (scoped; BA/PC currently empty) | query | 200 | `FacilityDto[]` | |
| GET | `api/Facilities/GetById/{id}` | logged-in (scoped) | — | 200 | `FacilityDto` | 404 |
| POST | `api/Facilities/AddFacility` | SA, OA | `branchId`, `addressId`, `typeId`, `partnerId` (all req), `startDate` (req), `arName`, `enName` (req, ≤200), `projectCode?` (≤50) — **no code** (server generates) | 201 | `FacilityDto` | 400 (domain), 404 (branch/address/type/partner), 409 (code generation retries exhausted) |
| PUT | `api/Facilities/UpdateFacility/{id}` | SA, OA | `addressId`, `typeId` (req), `arName`, `enName` (req) | 200 | `FacilityDto` | 400, 404 |
| POST | `api/Facilities/deactivateFacility/{id}` | SA, OA | — | 204 | — | 400, 404 |

## 6. Facility partners (history) — `api/facility-partners` (**SA only**, controller-level)

| Method | Route | Request | Success | Response | Errors |
|---|---|---|---|---|---|
| GET | `api/facility-partners/GetByFacility/{facilityId}` | — | 200 | `FacilityPartnerDto[]` (newest first) | |
| POST | `api/facility-partners/Assign` | `facilityId`, `partnerId` (req), `projectCode?` (≤50), `startDate` (req) | 201 | `FacilityPartnerDto` | 400 (start date must be later than current), 404 (facility/partner) |

Assign closes the current assignment with `endDate = startDate - 1` and switches the facility's current partner.

## 7. Geography — `api/Geography` (all GET, logged-in)

| Method | Route | Response |
|---|---|---|
| GET | `api/Geography/Countries` | `CountryDto[]` |
| GET | `api/Geography/countries/{countryId}/cities` | `CityDto[]` (incl. `cCode`) |
| GET | `api/Geography/cities/{cityId}/regions` | `RegionDto[]` |
| GET | `api/Geography/cities/{cityId}/areas` | `AreaDto[]` |
| GET | `api/Geography/regions/{regionId}/districts` | `DistrictDto[]` |
| GET | `api/Geography/districts/{districtId}/areas` | `AreaDto[]` |

DTOs carry only ids + names + code (+ `cCode` for cities). `includeInactive` params are accepted but ignored ([OQ-19](../OPEN-QUESTIONS.md#oq-19)). 404 if a parent city doesn't exist (areas by city).

## 8. Addresses — `api/Addresses`

| Method | Route | Roles | Request | Success | Response | Errors |
|---|---|---|---|---|---|---|
| GET | `api/Addresses/GetByAreaId/{areaId}?includeInactive=` | logged-in | query | 200 | `AddressDto[]` | |
| GET | `api/Addresses/GetById/{id}` | logged-in | — | 200 | `AddressDto` | 404 |
| POST | `api/Addresses/Add` | SA, OA | `areaId` (req), `arName`, `enName` (req, ≤200) | 201 | `AddressDto` | 400, 404 (area), 409 (same-name per area) |
| PUT | `api/Addresses/Update/{id}` | SA, OA | `arName`, `enName` (req) | 200 | `AddressDto` | 400, 404, 409 |

No deactivate endpoint ([OQ-17](../OPEN-QUESTIONS.md#oq-17)).

## 9. Lookup-type controllers (same pattern)

Controllers with **kebab-case route + action routes**: `api/accommodation-types`, `api/disability-types`, `api/education-types`, `api/facility-types`, `api/identity-types`, `api/nationalities`, `api/partners`:

| Method | Route | Roles | Request | Success | Response | Errors |
|---|---|---|---|---|---|---|
| GET | `<base>/GetAll` (facility-types also `?includeInactive=`) | logged-in (anonymous-visible via fallback? no — fallback requires auth) | query | 200 | `TypeDto[]` | |
| POST | `<base>/Add` | SA | per type below | 201 | `TypeDto` | 400, 409 (code/en-name exists) |
| PUT | `<base>/Update` | SA | Id + fields below | 200 | `TypeDto` | 400, 404, 409 |

Type-specific fields:

| Lookup | Add/Update fields | DTO fields |
|---|---|---|
| Accommodation, Disability, Education types | `arName`, `enName` (≤100) | `id`, `arName`, `enName` |
| Nationalities | `arName`, `enName` (≤100) | `id`, `arName`, `enName` |
| FacilityTypes | `arName`, `enName` (≤200 in validator; ≤100 in DB — [OQ note](../OPEN-QUESTIONS.md#odds-and-ends-low-priority)), `code` (req) | `id`, `arName`, `enName`, `code` |
| IdentityTypes | `arName`, `enName` (≤100), `code` (req ≤30) | `id`, `arName`, `enName`, `code`, `valueRegex` |
| Partners | `arName`, `enName` (≤100), `code` (req, ≤10) | `id`, `arName`, `enName`, `code` |

## 10. Beneficiaries — `api/Beneficiaries`

| Method | Route | Policy roles | Request | Success | Response | Errors |
|---|---|---|---|---|---|---|
| POST | `api/Beneficiaries/Search` | SearchBeneficiaries (SA, FA, DE) | `fullCode?` **or** `firstName` + `lastName` (each ≥2 normalized chars), all ≤100 | 200 | `{ items: BeneficiarySearchItemDto[], hasMore }` (max 20) | 400 (validation) |
| POST | `api/Beneficiaries/Register` | **DE only** `[BUILT]` ([OQ-12](../OPEN-QUESTIONS.md#oq-12)) | fields below | 200 | `RegisterBeneficiaryResultDto` | 400 (validation, domain, identity regex), 403 (no facility), 409 (unique race) |
| GET | `api/Beneficiaries/GetAll?page=&facilityId=` | ViewFacilityBeneficiaries (SA, FA, DE) | query | 200 | `PagedResult<BeneficiaryListItemDto>` (pageSize 30, newest first) | 400 (SA without facilityId), 403 (no facility for FA/DE) |
| GET | `api/Beneficiaries/PendingReview` | ReviewBeneficiaryMatches (SA, FA) | — | 200 | `PendingBeneficiaryDto[]` (oldest first; SA = all facilities) | 403 |
| GET | `api/Beneficiaries/{id}/Identities` | ViewFacilityBeneficiaries | — | 200 | `BeneficiaryIdentityDto[]` | 404 (out-of-scope → 404) |
| GET | `api/Beneficiaries/{id}/MatchCandidates` | ReviewBeneficiaryMatches | — | 200 | `BeneficiaryMatchDto[]` (with score/status/reasons) | 400 (no pending review), 404 |
| POST | `api/Beneficiaries/{id}/ConfirmSame` | ReviewBeneficiaryMatches | `existingBeneficiaryId` (req, ≠ id, must be a candidate) | 204 | — | 400, 404 |
| POST | `api/Beneficiaries/{id}/ConfirmNotAMatch` | ReviewBeneficiaryMatches | `reason?` (≤500) | 204 | — | 400, 404 |
| DELETE | `api/Beneficiaries/{id}` (soft delete) | DeactivateBeneficiaries (SA, FA, DE) | — | 204 | — | 400, 404 |

`RegisterBeneficiaryRequest` fields (all not-null unless marked): `firstName`, `fatherName`, `lastName`, `motherName` (each ≤100 and must be non-empty after Arabic normalization), `birthDate` (`yyyy-MM-dd`; year ≥ 1900, not in future), `isBirthDateEstimated` (bool), `gender` (bool, true=male), `enrollmentDate`, `areaId`, `nationalityId`, `identityTypeId`, `identityValue?` (≤30; regex of the type applies).

## 11. Response DTO quick reference

| DTO | Fields |
|---|---|
| `LoginResultDto` | `token, refreshToken?, expiresAt, userId, userName, firstName, lastName, role, organizationId?, facilityId?, isMobile` |
| `UserDto` | `id, userName, firstName, lastName, role, organizationId?, facilityId?, isMobile, isActive, createdAt` |
| `OrganizationDto` | `id, arName, enName, code, isActive, createdAt, updatedAt?` |
| `BranchDto` | `id, arName, enName, organizationId, isActive, createdAt, updatedAt?` |
| `FacilityDto` | `id, branchId, addressId, arName, enName, code, isActive, typeId, createdAt, updatedAt?` |
| `FacilityPartnerDto` | `id, facilityId, partnerId, partnerArName, partnerEnName, projectCode, startDate, endDate?, isCurrent` |
| `AddressDto` | `id, areaId, arName, enName, isActive, createdAt, updatedAt?` |
| `BeneficiarySearchItemDto` | `id, fullCode, firstName, fatherName, lastName, birthDate, isBirthDateEstimated, gender, identityTypeCode` (no identity value, no mother name) |
| `BeneficiaryListItemDto` | adds `motherName, enrollmentDate, areaId, matchReviewStatus, identity{...}` |
| `BeneficiaryIdentityDto` | `id, identityTypeId, identityTypeCode, identityTypeArName, identityTypeEnName, value` |
| `BeneficiaryMatchDto` | `beneficiaryId, fullCode, firstName, fatherName, lastName, motherName, birthDate, isBirthDateEstimated, gender, status, score, reasons, identity` |
| `PendingBeneficiaryDto` | `id, fullCode, ...demographics..., enrollmentDate, createdAt, identity` |
| `RegisterBeneficiaryResultDto` | `id, code, fullCode, matchReviewStatus` |
| `PagedResult<T>` | `items, page, pageSize, totalCount, totalPages, hasPreviousPage, hasNextPage` |

Enum values used in DTOs: see [enums-and-lookups.md](enums-and-lookups.md).
