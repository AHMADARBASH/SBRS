# SBRS API Conventions
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

Conventions every client (web, mobile) must follow. Verified against the code unless tagged otherwise.

## 1. Base URL and environments

| Environment | Base URL | Notes |
|---|---|---|
| Development (HTTP) | `<DEV_BASE_URL>` (launch profile default `http://localhost:5261`) | |
| Development (HTTPS) | `<DEV_BASE_URL>` (launch profile `https://localhost:7105`) | |
| Production | `<PROD_BASE_URL>` | served behind a reverse proxy; Swagger UI is currently enabled in Production too (see [OPEN-QUESTIONS.md](../OPEN-QUESTIONS.md#oq-25)) |

All routes are mounted under `api/…` (with the casing exactly as listed in [endpoint-reference.md](endpoint-reference.md)).

## 2. Authentication

- Every endpoint requires a **bearer token** unless listed as anonymous in [endpoint-reference.md](endpoint-reference.md). This is enforced by the ASP.NET authorization *fallback policy*, so forgetting an attribute does not open an endpoint.
- Header: `Authorization: Bearer <access_token>`. In Swagger UI paste the token **without** the `Bearer ` prefix.
- Full details: [auth-guide.md](auth-guide.md).

## 3. Request/response format

| Topic | Rule |
|---|---|
| Content type | `application/json` |
| Field naming | **camelCase** in JSON (ASP.NET Core default). Beware fields that are lowercase in C# (e.g. `typeId`, `branchId`, `organizationId` on some DTOs) — they serialize camelCase and therefore look identical; see [OPEN-QUESTIONS.md](../OPEN-QUESTIONS.md#oq-20) |
| Ids | GUIDs, 128-bit, lowercase textual form accepted (`"3fa85f64-5717-4562-b3fc-2c963f66afa6"`) |
| Dates | `date` fields are ISO 8601 `yyyy-MM-dd` (e.g. `"2015-03-01"`). Birth dates may have `isBirthDateEstimated = true` (store the supplied date; the server resolves non-estimated rules) |
| Date/time | UTC, ISO 8601 (`timestamps` in PostgreSQL wire as `yyyy-MM-ddTHH:mm:ss[.fff]Z`). `createdAt`/`updatedAt` are server-generated |
| Gender | **boolean; `true` (1) = male, `false` (0) = female** `[CONFIRMED][BUILT]` |
| Empty body responses | `204 No Content` for all deactivate/logout/confirm endpoints |
| Creation responses | `201 Created` with the created DTO (some via `Location`, some direct) |

## 4. Soft delete and `includeInactive`

- Records are **never deleted**; deactivation sets `is_active = false` `[BUILT]`.
- Most list endpoints return active records only; several accept `?includeInactive=true`:
  - truly honored: `GET api/Facilities/GetByBranchId/{branchId}`, `GET api/Addresses/GetByAreaId/{areaId}`, `GET api/organizations/GetAll`
  - accepted but currently **ignored**: `facility-types`, identity/geography subsets, `branches/GetAll` → tracked in [OPEN-QUESTIONS.md](../OPEN-QUESTIONS.md#oq-19)
- The beneficiary list never shows inactive records.

## 5. Scoping and the 404 rule

- Each user sees only their scope (SystemAdmin: all; OrganizationAdmin: own organization; FacilityAdmin/DataEntry: own facility; BranchAdmin/ProjectCoordinator: not yet wired — empty results).
- **Records outside the caller's scope are returned as `404 Not Found`, not `403`** `[CONFIRMED][BUILT]` — the frontend cannot enumerate ids it is not allowed to see. Show "not found", do not retry.
- A `403 Forbidden` means the *role* is wrong for the endpoint (policy check), not the data.

## 6. Pagination

Format used by `GET api/Beneficiaries/GetAll` (`PagedResult<T>`) `[BUILT]`:

```json
{
  "items": [ /* page items */ ],
  "page": 1,
  "pageSize": 30,
  "totalCount": 128,
  "totalPages": 5,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

- Page size is fixed at **30** for beneficiaries. `page` starts at 1 (validator: 1..100000).
- Only beneficiaries are paginated today; other lists return complete arrays `[BUILT]`.

## 7. Validation and errors

- All errors are RFC 7807 `application/problem+json`; see [error-catalog.md](error-catalog.md).
- Validation failures (FluentValidation) return `400` with an `errors` dictionary of field → messages.
- Common domain messages live in the error catalog's business-rule table.

## 8. Anonymous endpoints

Exactly four anonymous routes (all others require the bearer token) `[BUILT]`:

| Route | Purpose |
|---|---|
| `POST api/Auth/login` | get access + refresh token |
| `POST api/Auth/refresh` | rotate refresh token, get a new access token |
| `POST api/Auth/logout` | revoke one refresh token (takes it from body or cookie) |
| `GET /swagger/v1/swagger.json` (+ Swagger UI) | OpenAPI document |

`POST api/Auth/logout-all` and `GET api/Auth/me` require authentication.
