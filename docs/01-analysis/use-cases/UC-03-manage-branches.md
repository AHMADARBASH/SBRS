# UC-03 Manage branches
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Branches |
| Primary actor | SystemAdmin, OrganizationAdmin |
| Implementation status | `[BUILT]` (deactivate `[PLANNED]` — OQ-16) |
| Related rules | BR-BRN-01, BR-BRN-02, BR-BRN-03, BR-BRN-04, BR-BRN-05, BR-GEN-03 |
| Endpoints | `GET api/Branches/GetAll`, `GET api/Branches/GetById/{id}`, `GET api/Branches/GetByOrganizationId/{organizationId}`, `POST api/Branches/Add`, `PUT api/Branches/Update/{id}` |

## Goal
Create and maintain the branches of an organization; the HQ is just the first branch (BR-BRN-02).

## Preconditions
At least one organization exists (UC-02). Actor: SA anywhere, OA within their own organization.

## Main flow
1. OA (or SA) opens the branch list filtered by organization (`GetByOrganizationId`).
2. Create: `organizationId`, `arName`, `enName` → `201` with DTO (code optional per current API — unique per org when given).
3. Rename via `Update/{id}` (`arName`, `enName` only — the parent organization is immutable, BR-GEN-03).
4. List endpoints are scoped: OA reads a foreign org's branches as empty/404.

## Alternative flows
- A1 SA creates a branch directly under any organization from a global view (`GetAll`).

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| Unknown organizationId | `404` |
| Duplicate (org, code) | `409` unique violation |
| Update deactivated branch | `400` "A deactivated branch cannot be updated." |
| Deactivate with active facilities | guard exists (`HasActiveFacilitiesAsync`) but **no endpoint** (OQ-16) → impossible today |
| `GetById/{id}` response shape | returns a **single-element list** `[BUILT]` (OQ-26) |

## Postconditions
- Branch visible in the org tree; facilities can be added (UC-04).

## Screens (for frontend)
- Organization tree (org → branches → facilities), branch form.
