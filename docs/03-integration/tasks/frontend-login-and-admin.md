# Frontend task — Login & SystemAdmin area

> **Status:** Ready to implement · **Last updated:** 2026-11-15 · **Owner:** Frontend (handoff from backend)
>
> Every endpoint in this document exists in the running API. The former blockers are built:
> `GET api/Users/GetAll` (paged user list), `valueRegex` on identity-type Add/Update, and BranchAdmin
> creation/scoping (`branchId` on users, `branch_id` claim, branch-scoped reads).
> Field lists, routes and rules below were read from the controllers/contracts — not from memory.

---

## 1. Scope

**In scope**

1. **Login screen** for every user + full session handling (silent refresh, F5 survival, logout).
2. **SystemAdmin area** with five sections:
   - **Users** — list, filter, create, edit, deactivate, reset password, assign role/scope.
   - **Organizations** — list, create, edit.
   - **Branches** — list per organization, create, edit.
   - **Facilities** — list by branch, create (pick an area), edit, deactivate/activate.
   - **Lookup tables** — one generic screen for the editable lookup endpoints + read-only lists.

**Out of scope (separate tasks):** beneficiary registration/search/match review, beneficiary profile,
reports, pharmacy. The user's workspace for non-admin roles is only a **placeholder** in this task.

**Access model:** the admin area is **SystemAdmin (SA)** only. (`OrganizationAdmin` will reuse the
Organization/Branch/Facility screens later; its read endpoints already work, its Add/Update policies are
checked server-side — show buttons only for SA in this task.)

---

## 2. Environment & conventions

| Item | Value |
|---|---|
| Dev base URL | `https://localhost:<port>` (see `launchSettings.json`; Swagger at `/swagger`) |
| Production base URL | `https://sbrs.sarc-sy.org` (nginx → API) |
| JSON | `camelCase` keys, dates ISO-8601 UTC (`2026-11-15T08:00:00Z`), enums as strings |
| Auth header | `Authorization: Bearer <access token>` on every call except `Login`/`Refresh`/`Logout` |
| Client header | `X-Client-Type: web` on `Login`, `Refresh`, `Logout` (switches the refresh token to the HttpOnly cookie) |
| Cookies | every request: `credentials: "include"` (fetch) / `withCredentials: true` (axios) |
| Error body | RFC 7807 problem details: `{ "type", "title", "status", "detail", "errors"?, "traceId" }` |

Rules that prevent most frontend bugs:

- **Never hardcode ids.** Ids differ between environments; **`code` fields are the stable keys**
  (e.g. identity type `NATIONAL_ID`, facility type `HEALTH`).
- **Bilingual:** every editable entity has `arName` + `enName`, both required. Render Arabic RTL.
- **Deactivation is soft.** Deactivated rows stay in the DB; hide them by default, show behind a
  "show inactive" toggle where the endpoint supports it.
- **Never store the access token in localStorage/sessionStorage** (see §3).

---

## 3. Part 1 — Login & session

### 3.1 Flow

1. `POST api/Auth/Login` with `{ userName, password }`, headers `Content-Type: application/json` and
   `X-Client-Type: web`, credentials included.
2. On 200: keep `token` + `expiresAt` **in memory only**. The response has **no `refreshToken`** for web —
   it was set as the HttpOnly cookie `sbrs_rt` (`Path=/api/Auth`, `Secure`, `SameSite=Lax`).
3. Immediately call `GET api/Auth/Me` (Bearer token) and store the returned session object.
4. Route by role:
   - `SystemAdmin` → the admin area (§4).
   - Every other role → their workspace placeholder; if `facility` is present, route by
     **`facility.type.code`** (see §3.3).
5. On F5: memory is empty → call `POST api/Auth/Refresh` **first** (cookie rides along); on 200 store the
   new token and continue; on 401 → login screen.

### 3.2 Endpoints

| Method | Route | Auth | Body | Success | Notes |
|---|---|---|---|---|---|
| POST | `api/Auth/Login` | public | `{ userName, password }` | 200 `LoginResultDto` | 401 for wrong password / inactive / locked / no role — **identical body**, show one generic message |
| POST | `api/Auth/Refresh` | public (cookie) | empty | 200 `LoginResultDto` | rotates both tokens; a **new cookie** is set |
| POST | `api/Auth/Logout` | public (cookie) | empty | 204 | revokes the refresh token and deletes the cookie |
| POST | `api/Auth/LogoutAll` | logged-in | — | 204 | revokes every refresh token of the user ("log out everywhere") |
| GET | `api/Auth/Me` | logged-in | — | 200 `CurrentUserDto` | full session object (§3.3) |

`LoginResultDto`:

```json
{
  "token": "<JWT>",
  "refreshToken": null,
  "expiresAt": "2026-11-15T12:00:00Z",
  "userId": "…", "userName": "fa.damascus",
  "firstName": "Ahmad", "lastName": "Arbash",
  "role": "FacilityAdmin",
  "organizationId": "…", "branchId": null, "facilityId": "…",
  "isMobile": false
}
```

### 3.3 `GET api/Auth/Me` — use it for everything after login

```json
{
  "userId": "…", "userName": "fa.damascus",
  "firstName": "Ahmad", "lastName": "Arbash",
  "role": "FacilityAdmin",
  "isSystemAdmin": false,
  "isMobile": false,
  "organizationId": "…", "branchId": null, "facilityId": "…",
  "organization": {
    "id": "…", "code": "SARC", "arName": "…", "enName": "…", "isActive": true
  },
  "facility": {
    "id": "…", "arName": "…", "enName": "…", "code": "DAM-PHC-000012", "isActive": true,
    "type":   { "id": "…", "code": "HEALTH", "arName": "صحي", "enName": "Health" },
    "branch": { "id": "…", "arName": "…", "enName": "…" }
  }
}
```

- `organization` is `null` for SA; own org for OA; the branch's org for BA; the facility's owning org for
  facility roles. `branchId` is set only for BranchAdmin.
- `facility` is `null` for SA, OA and BA.
- **Routing key = `facility.type.code`.** Keep a map in the frontend:

```ts
const WORKSPACE_BY_FACILITY_TYPE: Record<string, string> = {
  HEALTH: "/health",
  COMMUNITY_SERVICES: "/community-services",
  OPERATIONS: "/operations",
};
// unknown/missing code → neutral default workspace (never crash)
```

Read the real type codes from `GET api/FacilityTypes/GetAll` — do not hardcode ids or names.
The type is read from the DB on every `me` call, so a mid-session type change is reflected on refresh.

### 3.4 Refresh handling (critical)

- **Serialize refreshes:** keep exactly one pending refresh promise; concurrent 401s must await it.
  A second parallel refresh burns an already-used token → the server revokes the whole token family →
  the user is logged out everywhere.
- On 401 from any endpoint: refresh **once**, retry the original request **once**. If the refresh 401s:
  clear the in-memory session and go to login. Never loop.
- On 403: do **not** refresh, do **not** retry — hide/disable the action instead.
- Do not attach the Authorization header to `refresh`; the cookie is the credential.

```ts
let refreshing: Promise<string> | null = null;

async function getFreshToken(): Promise<string> {
  refreshing ??= (async () => {
    const res = await fetch(`${BASE}/api/Auth/Refresh`, {
      method: "POST",
      headers: { "X-Client-Type": "web" },
      credentials: "include",
    });
    if (!res.ok) throw new SessionExpiredError();
    const data = await res.json();
    session.token = data.token;
    session.expiresAt = data.expiresAt;
    return data.token as string;
  })().finally(() => { refreshing = null; });
  return refreshing;
}
```

### 3.5 Logout

`POST api/Auth/Logout` (`X-Client-Type: web`, credentials included) → 204 → clear the in-memory session →
navigate to login. Optional "log out everywhere" button → `POST api/Auth/LogoutAll` (needs the Bearer token).

---

## 4. Part 2 — SystemAdmin area

Suggested left nav: **Users · Organizations · Branches · Facilities · Lookup Tables**.
Header shows `me.firstName + me.lastName` and the logout actions.

### 4.1 Users — `api/Users` (entire controller SA only)

| Method | Route | Body / query | Success | Response |
|---|---|---|---|---|
| GET | `GetAll` | `page=1`, `pageSize=20` (1–100), `includeInactive=false`, `search?`, `role?` | 200 | `PagedResult<UserListItemDto>` |
| GET | `Roles` | — | 200 | `[{ role, requiresOrganizationId, requiresBranchId, requiresFacilityId }]` |
| POST | `CreateUser` | `{ userName, password, firstName, lastName, role, organizationId?, branchId?, facilityId?, isMobile }` | **201** | `UserDto` |
| PUT | `UpdateUser` | `{ id, firstName, lastName, role, organizationId?, branchId?, facilityId?, isMobile }` | 200 | `UserDto` |
| POST | `DeactivateUser/{id}` | — | 204 | — |
| POST | `ActivateUser/{id}` | — | 204 | — |
| POST | `ResetPassword` | `{ id, newPassword }` | 204 | — |

**List** — `GET api/Users/GetAll?page=1&pageSize=20&search=ahmad&role=FacilityAdmin`:

```json
{
  "items": [
    {
      "id": "…",
      "userName": "fa.damascus",
      "firstName": "Ahmad", "lastName": "Arbash",
      "role": "FacilityAdmin",
      "isMobile": false,
      "isActive": true,
      "createdAt": "2026-11-15T08:00:00Z",
      "organization": { "id": "…", "code": "SARC", "arName": "…", "enName": "…", "isActive": true },
      "branch":     { "id": "…", "arName": "…", "enName": "…" },   // null unless the role has a branch
      "facility":   { "id": "…", "code": "DAM-PHC-000012", "arName": "…", "enName": "…", "isActive": true }
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 42, "totalPages": 3,
  "hasPreviousPage": false, "hasNextPage": true
}
```

- `search` matches user name, first name or last name (case-insensitive substring).
- `role` is an exact role name from `GetAll Roles`.
- Sorted newest first. `organization`/`branch`/`facility` are `null` when the role has no such scope.
- Inactive users are hidden unless `includeInactive=true`.

**Create/Edit form:** build it from `GET api/Users/Roles` so no role logic is hardcoded:

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

- Show the organization picker only when `requiresOrganizationId`; the branch picker only when
  `requiresBranchId`; the facility picker only when `requiresFacilityId` (pick a branch in §4.3, then its
  facilities in §4.4). Hide the others. A BranchAdmin sends **only** `branchId` — the organization is
  derived server-side from the branch, so an organization sent alongside it is ignored.
- **Password policy** (server-enforced by ASP.NET Identity): **≥ 8 chars with at least one uppercase,
  one lowercase, one digit and one non-alphanumeric character.** Mirror it in the form and show a hint.
- `userName`: 3–50 chars, only `A-Z a-z 0-9 . _ -`.
- Deactivation: confirm dialog; the server refuses deactivating **your own account** and the **SystemAdmin
  account** (target check), and revokes that user's sessions.
- **Reactivation:** inactive rows (visible with `includeInactive=true`) show an "Activate" action →
  `POST api/Users/ActivateUser/{id}` → 204 → refetch. Activating does not restore a forgotten password or
  clear a lockout — those are handled by `ResetPassword`.
- Changing role/scope or resetting a password **revokes that user's sessions** server-side.

`UserDto` (single-user responses):

```json
{ "id": "…", "userName": "…", "firstName": "…", "lastName": "…",
  "role": "FacilityAdmin", "organizationId": "…", "branchId": null, "facilityId": "…",
  "isMobile": false, "isActive": true, "createdAt": "…" }
```

### 4.2 Organizations — `api/Organizations`

| Method | Route | Body | Access | Notes |
|---|---|---|---|---|
| GET | `GetAll` | — | logged-in | **Active only** (no toggle param) |
| GET | `GetById/{id}` | — | logged-in | 404 when missing |
| POST | `Add` | `{ arName, enName, code }` | SA | `code` ≤ 10, unique (409) |
| PUT | `Update/{id}` | `{ arName, enName, code }` | SA/OA | code also required and unique |

There is **no deactivate endpoint** for organizations (commented out in the controller) — do not build one.

### 4.3 Branches — `api/Branches`

| Method | Route | Body | Access |
|---|---|---|---|
| GET | `GetAll?includeInactive=` | — | logged-in |
| GET | `GetById/{id}` | — | logged-in |
| GET | `GetByOrganizationId/{organizationId}` | — | logged-in |
| POST | `Add` | `{ organizationId, arName, enName }` | SA/OA |
| PUT | `Update/{id}` | `{ arName, enName }` | SA/OA |

⚠️ `Branches/GetAll` ignores the `includeInactive` flag; filter client-side on `isActive` for the "show
inactive" toggle. It is **scoped**: SA sees all branches, OA its organization's, **BranchAdmin only its own
branch**, FA/DE their organization's. `GetById` for an out-of-scope id returns 404. `Update` cannot move a
branch to another organization — hide the organization field when editing.

### 4.4 Facilities — `api/Facilities`

| Method | Route | Body / query | Access |
|---|---|---|---|
| GET | `GetByBranchId/{branchId}?includeInactive=` | — | logged-in (scoped: SA all, OA its org, BA its own branch, FA/DE own facility, PC nothing) |
| GET | `GetById/{id}` | — | logged-in |
| POST | `AddFacility` | see below | SA/OA |
| PUT | `UpdateFacility/{id}` | `{ arName, enName, areaId, typeId, categoryId? }` | SA/OA |
| POST | `DeactivateFacility/{id}` | — | SA/OA |
| POST | `ActivateFacility/{id}` | — | SA/OA |

`AddFacility` body:

```json
{
  "branchId": "…", "areaId": "…", "typeId": "…",
  "partnerId": "…", "startDate": "2026-11-15", "projectCode": "PRJ-01",
  "arName": "…", "enName": "…", "categoryId": "…"
}
```

- `categoryId` is optional (a `facility_categories` id from `GET api/FacilityCategories/GetAll`).
  Omit it or send `null` to create/update without a category; on update `null` clears it.

- **The facility `code` is generated by the server** (`{CityCCode}-{FacilityTypeCode}-{NNN}`) — never show
  a code input on create. The code is immutable; `UpdateFacility` has no code field.
- A facility is always created **with a partner assignment**: `partnerId`, `startDate` (date only) and
  optional `projectCode` are required by the API, and this creates the facility–partner history row.
  Populate the partner picker from `api/Partners/GetAll`, and the type picker from
  `api/FacilityTypes/GetAll`.
- Facility create needs an `areaId` → pick it directly from the geography cascade (§4.7); facilities no longer reference addresses.

`FacilityDto` returned by list/detail:

```json
{ "id": "…", "branchId": "…", "areaId": "…",
  "arName": "…", "enName": "…", "code": "DAM-PHC-000012",
  "isActive": true, "typeId": "…", "createdAt": "…", "updatedAt": null }
```

(Its `typeId` is not enriched here — resolve the type name from `api/FacilityTypes/GetAll` if the screen
shows it. The nested `facility.type` object exists only on `api/Auth/Me`.)

Optional stretch — facility partners (`api/FacilityPartners`, **SA only**):
`GET GetByFacility/{facilityId}` and `POST Assign { facilityId, partnerId, projectCode?, startDate }` (201).

### 4.5 Lookup tables — one generic screen

Use a small config array; **all follow the same pattern**: `GET <base>/GetAll` (any logged-in user),
`POST <base>/Add` (SA, 201), `PUT <base>/Update` (SA, 200). DTO: `{ id, code?, arName, enName }`.

| Base route | Add/Update fields | Notes |
|---|---|---|
| `api/AccommodationTypes` | `arName`, `enName` | no code |
| `api/DisabilityTypes` | `arName`, `enName` | no code |
| `api/EducationTypes` | `arName`, `enName` | no code |
| `api/Nationalities` | `arName`, `enName` | no code |
| `api/FacilityTypes` | `arName`, `enName`, `code` | `GetAll?includeInactive=` supported |
| `api/IdentityTypes` | `arName`, `enName`, `code`, **`valueRegex`** | see below |
| `api/Partners` | `arName`, `enName`, `code` | `GetAll` only on this controller |

**Identity types — `valueRegex` (now editable):**

- Request (Add; Update adds `id`):
  ```json
  { "arName": "هوية وطنية", "enName": "National ID", "code": "NATIONAL_ID", "valueRegex": "^[0-9]{10}$" }
  ```
- `valueRegex` is **required**, max **200 chars**, must be a **valid .NET regular expression**, otherwise
  400 with a field error. It is the pattern a beneficiary's identity value must match at registration
  (after trimming + Arabic-Indic digit normalization).
- Render the field in the form only for identity types; help text example: `^[0-9]{10}$` = exactly 10 ASCII
  digits; `^$` = value must be empty (`UNREGISTERED`).
- In .NET `\d` matches Arabic-Indic digits — prefer `[0-9]` in patterns.
- Editing the regex affects future registrations immediately; existing stored values are not re-validated.

**Read-only lookups (GetAll only — render no edit affordances):**
`api/BeneficiaryStatuses`, `api/MaritalStatuses`, `api/DisabilityReasons`, `api/ChronicDiseases`,
`api/EducationIssues`. Shape `{ id, code, arName, enName }`, ordered by code.

### 4.6 Addresses — `api/Addresses`

| Method | Route | Body | Access |
|---|---|---|---|
| GET | `GetByAreaId/{areaId}?includeInactive=` | — | logged-in |
| GET | `GetById/{id}` | — | logged-in |
| POST | `Add` | `{ areaId, arName, enName }` | SA/OA |
| PUT | `Update/{id}` | `{ arName, enName }` | SA/OA |

### 4.7 Geography pickers — `api/Geography` (all GET, any logged-in user)

```
Countries
Countries/{countryId}/Cities
Cities/{cityId}/Regions
Cities/{cityId}/Areas
Regions/{regionId}/Districts
Districts/{districtId}/Areas
```

Chain them for the address cascade (country → city → region/district → area). Do **not** cache across
sessions in a way that prevents new areas from appearing.

---

## 5. Forms & validation quick reference

| Entity | Field | Rule |
|---|---|---|
| User | `userName` | 3–50, `A-Za-z0-9._-` |
| User | `password` / `newPassword` | ≥8, upper + lower + digit + non-alphanumeric |
| User | `role` | one of `GetAll Roles`; scope picker per `requiresOrganizationId` / `requiresBranchId` / `requiresFacilityId` |
| Organization | `arName`, `enName` | required, ≤50 |
| Organization | `code` | required, ≤10, unique (409) |
| Branch | `arName`, `enName` | required, ≤50 |
| Branch | `organizationId` | required on create only |
| Address | `arName`, `enName` | required |
| Address | `areaId` | required on create |
| Facility | `arName`, `enName` | required, ≤200 (DB caps at 100 — stay ≤100) |
| Facility | `branchId`, `areaId`, `typeId`, `partnerId`, `startDate` | required on create |
| Facility | `code` | **server-generated, read-only** |
| Lookup (all) | `arName`, `enName` | required, ≤100 |
| Lookup with code | `code` | required, ≤30 (partners ≤10), unique (409) |
| Identity type | `valueRegex` | required, ≤200, valid .NET regex |

---

## 6. Cross-cutting UX rules

1. **List pages:** pagination (Users only — the other lists are unpaged arrays), filters, empty state.
2. **"Show inactive" toggle** where supported: Users (`includeInactive`), Facilities
   (`includeInactive`), Facility types (`includeInactive`), Addresses (`includeInactive`).
   Organizations show active only; Branches need client-side filtering.
3. **Buttons by permission:** hide Add/Edit/Deactivate unless the user is SystemAdmin. The backend answers
   403 regardless — never rely on the UI for security.
4. **Confirmation dialogs** for every deactivate action; explain that it is a soft delete.
5. **Optimistic-free:** after create/update, refetch or merge the returned DTO. On 409 show the conflict
   message from `detail` (unique code/name).
6. **Do not leak profile data** anywhere in these screens (they don't touch beneficiaries).

---

## 7. Error handling

| Status | Meaning | Frontend action |
|---|---|---|
| 400 | Validation / domain rule / identity regex | Show `detail` or field `errors`; keep the form open |
| 401 | Missing/expired access token | Refresh once (serialized), retry once; refresh failed → login |
| 403 | Role not allowed | Do not retry; hide/disable the action |
| 404 | Missing or out of scope | Show "not found"; refresh the list |
| 409 | Unique constraint (code / name / user name) | Show the conflict message |
| 500 | Unexpected | Generic message + `traceId` reference |

Reference: `docs/03-integration/error-catalog.md`.

---

## 8. Acceptance criteria

- [ ] Login works on production (`https://sbrs.sarc-sy.org`); cookie `sbrs_rt` visible with
      `Path=/api/Auth`; F5 keeps the session through silent refresh.
- [ ] Deleting the cookie / forcing a 401 routes to login without a redirect loop.
- [ ] Two tabs refreshing simultaneously never log the user out (serialized refresh).
- [ ] `me` decides routing: SA → admin area; BranchAdmin (`branchId` set) → branch overview; facility users
      → workspace chosen by `facility.type.code`; unknown code → neutral workspace.
- [ ] SA can list users (search + role filter + inactive toggle) and sees role and scope names without
      extra calls.
- [ ] SA can create a user for every role with the correct required id (including a BranchAdmin whose
      organization is derived from the chosen branch); reset a password; deactivate a user;
      and reactivate a deactivated user (reappears as active in the list).
- [ ] SA can add/edit organizations, branches, facilities (with area + partner), and deactivate/activate a facility.
- [ ] SA can add/edit every editable lookup; identity type form includes `valueRegex` with client-side
      regex validation and server 400 shown on invalid patterns.
- [ ] Read-only lookups and Organizations render no edit affordances.
- [ ] Facility creation shows the server-generated code read-only.
- [ ] Deactivated users/facilities/lookups disappear from default lists and reappear behind the toggle.

---

## 9. Appendix — route index

```
POST   api/Auth/Login                 POST   api/Auth/Refresh
POST   api/Auth/Logout                POST   api/Auth/LogoutAll
GET    api/Auth/Me

GET    api/Users/GetAll               GET    api/Users/Roles
POST   api/Users/CreateUser           PUT    api/Users/UpdateUser
POST   api/Users/DeactivateUser/{id}  POST   api/Users/ResetPassword
POST   api/Users/ActivateUser/{id}

GET    api/Organizations/GetAll       GET    api/Organizations/GetById/{id}
POST   api/Organizations/Add          PUT    api/Organizations/Update/{id}

GET    api/Branches/GetAll            GET    api/Branches/GetById/{id}
GET    api/Branches/GetByOrganizationId/{organizationId}
POST   api/Branches/Add               PUT    api/Branches/Update/{id}

GET    api/Facilities/GetByBranchId/{branchId}
GET    api/Facilities/GetById/{id}
POST   api/Facilities/AddFacility     PUT    api/Facilities/UpdateFacility/{id}
POST   api/Facilities/DeactivateFacility/{id}

GET    api/Addresses/GetByAreaId/{areaId}
GET    api/Addresses/GetById/{id}
POST   api/Addresses/Add              PUT    api/Addresses/Update/{id}

GET    api/Geography/Countries
GET    api/Geography/Countries/{countryId}/Cities
GET    api/Geography/Cities/{cityId}/Regions
GET    api/Geography/Cities/{cityId}/Areas
GET    api/Geography/Regions/{regionId}/Districts
GET    api/Geography/Districts/{districtId}/Areas

GET    api/AccommodationTypes/GetAll       POST Add   PUT Update
GET    api/DisabilityTypes/GetAll          POST Add   PUT Update
GET    api/EducationTypes/GetAll           POST Add   PUT Update
GET    api/FacilityTypes/GetAll            POST Add   PUT Update
GET    api/IdentityTypes/GetAll            POST Add   PUT Update
GET    api/Nationalities/GetAll             POST Add   PUT Update
GET    api/Partners/GetAll                  POST Add   PUT Update
GET    api/BeneficiaryStatuses/GetAll      (read-only)
GET    api/MaritalStatuses/GetAll          (read-only)
GET    api/DisabilityReasons/GetAll        (read-only)
GET    api/ChronicDiseases/GetAll          (read-only)
GET    api/EducationIssues/GetAll          (read-only)
GET    api/FacilityPartners/GetByFacility/{facilityId}   (SA)
POST   api/FacilityPartners/Assign                        (SA)
```
