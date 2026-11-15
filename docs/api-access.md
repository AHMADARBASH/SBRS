# SBRS API — Access & Roles Reference (for the frontend)

This file explains **who can call what** in the SBRS API and **what data follows from who you are**.
It was generated from the actual authorization code (`SBRS/Authorization/Policies.cs`, the controllers'
`[Authorize]` attributes, and the data scoping inside the query/command handlers), so it matches the
running API — not just intentions.

Generated: 2026-11-09.

---

## 1. How authentication works

- **Every endpoint requires a logged-in user** unless marked otherwise. This is a server-side
  *fallback policy*, so even an endpoint that forgot to declare an attribute stays protected.
- Send the access token on every call:
  `Authorization: Bearer <access_token>`
- Token comes from `POST /api/Auth/login` (web clients also receive an http-only refresh-token
  cookie; refresh via `POST /api/Auth/refresh`).
- Result codes:
  - **401 Unauthorized** — no token / invalid / expired token → the UI should redirect to login and
    call `POST /api/Auth/refresh` first when the web refresh cookie exists.
  - **403 Forbidden** — valid user, but their **role** is not allowed on this endpoint. The UI should
    hide/disable that feature instead of letting the user hit a 403.
- Running instance: live Swagger is still the best request/response reference —
  the API publishes OpenAPI/Swagger at `/swagger`.

## 2. The roles

Roles come back as plain strings (`SystemAdmin`, `OrganizationAdmin`, …) in the JWT role claim and in
`GET /api/Auth/me`.

| Role | Meaning | Scope of the data it sees |
|---|---|---|
| `SystemAdmin` | Super-admin of the whole system. Has **no facility** (it manages facilities/branches/users and can pick any facility when browsing). | Everything. |
| `OrganizationAdmin` | Manages one organization and its branches/facilities. Reads/writes are limited to its `organizationId` (from the token). | Its organization only. |
| `BranchAdmin` | Branch-level role. Has one `branchId` (and the branch's `organizationId`, derived server-side). | Its branch: the branch itself and the facilities under it. Report-only — no write endpoints. |
| `ProjectCoordinator` | Project-level role. | ⚠️ Not wired up yet — returns nothing in scoped queries (OQ-28). |
| `FacilityAdmin` | Manages one facility (has `facilityId` in the token). Sees/reviews that facility's beneficiaries, reviews match candidates. | Its facility only. |
| `DataEntry` | Terminal operator at a facility (has `facilityId`). Registers beneficiaries, searches, lists. | Its facility only. |

**How the frontend learns its own role & scope:** call `GET /api/Auth/me` after login. The response is the
**full session object**: the complete user plus the complete facility/organization context, with the child
data as **nested reusable objects** (`organization{}`, `facility.type{}`, `facility.branch{}`), so the client
shell needs no follow-up requests after login:

```json
{
  "userId": "...",
  "userName": "fa.damascus",
  "firstName": "...", "lastName": "...",
  "role": "FacilityAdmin",
  "isSystemAdmin": false,
  "isMobile": false,
  "organizationId": "...",          // scope ids, kept flat for preselect / backward compat
  "branchId": "...",                // set for BranchAdmin (and only then)
  "facilityId": "...",
  "organization": {                 // null for SystemAdmin; own org for OrganizationAdmin;
    "id": "...",                    // the owning org of the facility/branch for branch and facility roles
    "code": "SARC",
    "arName": "...", "enName": "...",
    "isActive": true
  },
  "facility": {                     // null when the role has no facility (SA/OA/BA)
    "id": "...",
    "arName": "...", "enName": "...",
    "code": "DAM-PHC-000012",
    "isActive": true,
    "type":   { "id": "...", "code": "HEALTH", "arName": "صحي", "enName": "Health" },   // ⭐ type.code = routing key
    "branch": { "id": "...", "arName": "...", "enName": "..." }
  }
}
```

Use `role` to decide which menu items/views to show, `facility.type.code` to route facility users to the
workspace matching their facility type (unknown code → neutral default workspace), and the flat
`organizationId` / `facilityId` ids to preselect scope fields. The type is read from the database on
every call — it is **not** a JWT claim — so a mid-session type change takes effect on the next `me`/refresh.
The nested objects are shared summary DTOs (`OrganizationSummaryDto`, `FacilityTypeSummaryDto`,
`BranchSummaryDto`) intended to appear in other endpoints too, so the frontend can reuse the same types.

---

## 3. Role × Endpoint matrix

⚠️ **Fallback policy:** every endpoint below is "logged-in users" at minimum. The *Access* column shows
the extra restriction on top.

Abbreviations: **SA**=SystemAdmin, **OA**=OrganizationAdmin, **BA**=BranchAdmin, **PC**=ProjectCoordinator,
**FA**=FacilityAdmin, **DE**=DataEntry.

### Auth — `api/Auth`

| Method | Path | Access | Notes |
|---|---|---|---|
| POST | `login` | 🌐 Public | Returns access token (+ refresh cookie for web clients / refresh token for mobile) |
| POST | `refresh` | 🌐 Public | Web: reads cookie; Mobile: refresh token in body |
| POST | `logout` | 🌐 Public | Web: revokes cookie token |
| GET | `me` | Any logged-in user | ⭐ Full session object: user + nested `organization{}` / `facility{}` (incl. `facility.type.code` routing key) |
| POST | `logout-all` | Any logged-in user | Revokes all refresh tokens |

### Users — `api/Users` (whole controller: **SA only**)

| Method | Path | Access | Notes |
|---|---|---|---|
| GET | `GetAll` | SA | Paged user list: `?page=1&pageSize=20&includeInactive=false&search=&role=`. Rows embed `organization{}` / `branch{}` / `facility{}` summaries |
| GET | `Roles` | SA | Which roles exist and which ids each needs (see §4) |
| POST | `CreateUser` | SA | Body needs `role` + `organizationId` (OrganizationAdmin) / `branchId` (BranchAdmin) / `facilityId` (FacilityAdmin, DataEntry) |
| PUT | `UpdateUser` | SA | Same role/id rules as create |
| POST | `deactivateUser/{id}` | SA | Soft-deactivates the user |
| POST | `activateUser/{id}` | SA | Re-enables a deactivated user (204; no-op if already active) |
| POST | `reset-password` | SA | Body: `{ id, newPassword }` |

### Organizations — `api/organizations`

| Method | Path | Access | Notes |
|---|---|---|---|
| GET | `GetAll` | Any logged-in user | |
| GET | `GetById/{id}` | Any logged-in user | |
| POST | `Add` | SA | |
| PUT | `Update/{id}` | SA, OA | |

### Branches — `api/Branches`

| Method | Path | Access | Notes |
|---|---|---|---|
| GET | `GetAll` | Any logged-in user | Scoped: SA all; OA its organization's branches; BA only its own branch; FA/DE their organization's branches |
| GET | `GetById/{id}` | Any logged-in user | Same scoping (out of scope → 404) |
| GET | `GetByOrganizationId/{organizationId}` | Any logged-in user | Same scoping |
| POST | `Add` | SA, OA | |
| PUT | `Update/{id}` | SA, OA | |

### Facilities — `api/Facilities`

| Method | Path | Access | Notes |
|---|---|---|---|
| GET | `GetByBranchId/{branchId}?includeInactive=` | Any logged-in user | Scoped: SA all; OA its organization; **BA its own branch**; FA/DE their own facility; PC nothing |
| GET | `GetById/{id}` | same as above | Same scoping |
| POST | `AddFacility` | SA, OA | Facility **code is generated by the server** (`{City}-{Type}-{NNN}`); never send/sketch a code field |
| PUT | `UpdateFacility/{id}` | SA, OA | Code is immutable — no code field in the request |
| POST | `deactivateFacility/{id}` | SA, OA | |

### Facility Partners — `api/facility-partners` (whole controller: **SA only**)

| Method | Path | Access |
|---|---|---|
| GET | `GetByFacility/{facilityId}` | SA |
| POST | `Assign` | SA |

### Addresses — `api/Addresses`

| Method | Path | Access |
|---|---|---|
| GET | `GetByAreaId/{areaId}` | Any logged-in user |
| GET | `GetById/{id}` | Any logged-in user |
| POST | `Add` | SA, OA |
| PUT | `Update/{id}` | SA, OA |

### Geography — `api/Geography` (all GET, any logged-in user)

- `Countries`
- `countries/{countryId}/cities`
- `cities/{cityId}/regions`
- `cities/{cityId}/areas`
- `regions/{regionId}/districts`
- `districts/{districtId}/areas`

(These feed the address pickers — freely callable by any logged-in role.)

### Lookup tables (same pattern in each controller)

Controllers: `api/accommodation-types`, `api/disability-types`, `api/education-types`,
`api/facility-types`, `api/identity-types`, `api/nationalities`, `api/partners` — **and the
profile lookups** `api/beneficiary-statuses`, `api/marital-statuses`, `api/disability-reasons`,
`api/chronic-diseases`, `api/education-issues` (GetAll only).

| Method | Path | Access |
|---|---|---|
| GET | `GetAll` (also `api/partners/GetAll`) | Any logged-in user |
| POST | `Add` | **SA only** |
| PUT | `Update` | **SA only** |

Editable fields: `{ arName, enName }` for accommodation/disability/education types and nationalities;
`{ arName, enName, code }` for facility types and partners; **`{ arName, enName, code, valueRegex }` for
identity types** — `valueRegex` is required, ≤200 chars, a valid .NET regex, and is the pattern registration
enforces on the document value (after trim + Arabic-Indic digit normalization). Changing it affects future
registrations immediately.

The profile lookups have **no Add/Update** — they are seeded by the server (DTO shape
`{ id, code, arName, enName }`, ordered by code).

### Beneficiaries — `api/Beneficiaries`

| Method | Path | Access | Notes |
|---|---|---|---|
| POST | `Search` | SA, FA, DE | Body: `{ fullCode?, firstName?, lastName? }` — need FullCode **or** both names (2+ chars each). Names go in the body on purpose (never in URLs/logs) |
| POST | `Register` | **DE only** | Registers at the DataEntry's **own facility** (from token). Body: names, birthDate, gender, enrollmentDate, `areaId`, `nationalityId`, `identityTypeId`, `identityValue`. Response contains `code` & `fullCode`. If `matchReviewStatus` = `PendingReview` → show the flag; record still created |
| GET | `GetAll?page=&facilityId=` | SA, FA, DE | FA/DE: always their own facility (any `facilityId` param is ignored). SA: **must** pass `facilityId` (error if missing). Page size is 30 |
| GET | `PendingReview` | SA, FA | Candidates waiting for match review |
| GET | `{id}/Identities` | SA, FA, DE | Identity records of the beneficiary |
| GET | `{id}/MatchCandidates` | SA, FA | Scored probable matches |
| POST | `{id}/ConfirmSame` { existingBeneficiaryId } | SA, FA | Merge the duplicate into the existing record. Must be one of the server's own candidates |
| POST | `{id}/ConfirmNotAMatch` { reason? } | SA, FA | Keep the new record as distinct |
| DELETE | `{id}` | SA, FA, DE | Soft delete — `is_active = false`; never a hard delete (codes are never reused). Returns 204 |
| GET | `GetProfile/{beneficiaryId}` | SA, FA, DE | The sensitive second step (phones, displacement, education, marital, disabilities, chronic diseases, status). Ids only — resolve names via the profile lookups. **404 when no profile was saved yet.** Same facility scoping as `{id}/Identities`. Never included in Search/GetAll/PendingReview/MatchCandidates |
| PUT | `SaveProfile/{beneficiaryId}` | SA, FA, DE | Create (201) or **full replace** (200). Body: `{ phoneNumber1?, phoneNumber2?, isDisplaced, displacementDate?, oldAreaId?, accommodationTypeId?, isDisabled, disabilities: [{disabilityTypeId, reasonId}], chronicDiseaseIds: [], beneficiaryStatusId?, currentWork?, previousWork?, educationTypeId?, educationIssueId?, maritalStatusId, wivesNumber?, childrenNumber? }` — omitted `beneficiaryStatusId` defaults to `NOT_AVAILABLE`; `accommodationTypeId`, `educationTypeId`, `maritalStatusId` are **nullable** (null = the answer is not known yet — sending wivesNumber/childrenNumber while `maritalStatusId` is unknown is rejected). Every field must be sent on replace |

---

## 4. Creating users (SystemAdmin screen)

`GET /api/Users/Roles` gives the current role list with the ids each role needs:

```json
[
  { "role": "SystemAdmin",        "requiresOrganizationId": false, "requiresBranchId": false, "requiresFacilityId": false },
  { "role": "OrganizationAdmin",  "requiresOrganizationId": true,  "requiresBranchId": false, "requiresFacilityId": false },
  { "role": "BranchAdmin",        "requiresOrganizationId": false, "requiresBranchId": true,  "requiresFacilityId": false },
  { "role": "ProjectCoordinator", "requiresOrganizationId": false, "requiresBranchId": false, "requiresFacilityId": false },
  { "role": "FacilityAdmin",      "requiresOrganizationId": false, "requiresBranchId": false, "requiresFacilityId": true  },
  { "role": "DataEntry",          "requiresOrganizationId": false, "requiresBranchId": false, "requiresFacilityId": true  }
]
```

Server-side validation mirrors this: the create is rejected when `organizationId` / `branchId` /
`facilityId` is missing for the chosen role, so build the form fields from these flags. For a
BranchAdmin send **only** `branchId`; the organization is always derived from that branch, so any
`organizationId`/`facilityId` sent alongside it is ignored (and the stored values come from the branch).

---

## 5. Practical tips for the frontend

1. **Never show a register/merge screen based on the URL only** — check `/api/Auth/me`'s `role`:
   `DataEntry` gets Register/Search, `FacilityAdmin` gets Register-review, `SystemAdmin` gets everything
   administrative, etc.
2. **Stores/filters by scope:** when the user is FA/DE, the backend ignores/forbids facility selection —
   don't render a facility picker for them; for **SA**, always send `facilityId`
   on beneficiaries `GetAll` (it errors without one).
3. **Facility code fields are output-only.** Facility codes are system-generated (e.g. `DAM-PHC-001`)
   and immutable; beneficiary `code`/`fullCode` likewise. Never render them as editable inputs.
4. **PendingReview flow:** `Register`'s response returns `matchReviewStatus`. On `PendingReview`
   the record exists and is listed for the facility admin under `PendingReview`, where
   `MatchCandidates` + `ConfirmSame`/`ConfirmNotAMatch` finish the story.
5. **403 handling:** show a "no permission" state instead of retrying; 401 → attempt refresh token flow
   once, then redirect to login.

---

*Keeping this doc in sync:* it mirrors three code sources — `Policies.Configure` (the role list per policy),
controller `[Authorize]` attributes, and repository scoping. If the backend changes access rules, update
this file too (or ask the backend to regenerate it).
