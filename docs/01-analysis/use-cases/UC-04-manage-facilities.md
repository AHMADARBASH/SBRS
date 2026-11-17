# UC-04 Manage facilities
> Status: Draft | Last updated: 2026-11-15 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Facilities (+ Facility types, Partners, Facility partners) |
| Primary actor | SystemAdmin, OrganizationAdmin |
| Implementation status | `[BUILT]` |
| Related rules | BR-FAC-01…10, BR-GEN-02…04, BR-GEO-01 |
| Endpoints | `GET/POST/PUT api/Facilities/…`, `POST api/Facilities/DeactivateFacility/{id}`, `POST api/Facilities/ActivateFacility/{id}`, `api/FacilityTypes/*`, `api/Partners/*`, `api/FacilityPartners/*` |

## Goal
Establish and maintain service points under branches, each with a system-generated readable code, an area, a type and a current partner plus partner history.

## Preconditions
Branch (UC-03), facility type (lookup), area (UC-06), partner all exist. Actor: SA (all) or OA (own organization).

## Main flow
1. OA opens a branch and adds a facility.
2. Form fields: `branchId`, `areaId`, `typeId`, `partnerId`, `startDate`, `arName`, `enName`, optional `projectCode`. **No code field** — the server generates `{city c_code}-{type code}-{NNN}` from the area's city (BR-FAC-02) and retries on any concurrent unique collision.
3. `201` returns the facility with its generated `code`.
4. Later edits change only `areaId`, `typeId`, names (`UpdateFacility/{id}`); branch and code are immutable.
5. Partner change history: `POST api/FacilityPartners/Assign` closes the open assignment (`endDate = startDate − 1 day`) and opens the new one; `GET GetByFacility/{facilityId}` shows the full history, newest first `[BUILT]`.
6. Retired facility: `POST DeactivateFacility/{id}` → 204 (soft delete; list `?includeInactive=true` reveals it).
7. Reactivated facility: `POST ActivateFacility/{id}` → 204; allowed only while the facility's branch is active.

## Alternative flows
- A1 SA manages facility types (`api/FacilityTypes/Add`, `Update`) — insert-only lookups, never deleted.
- A2 ProjectCode is optional; empty string is stored (BR-FAC-05).

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| Branch deactivated | `400` "Cannot add a facility to a deactivated branch." |
| Unknown branch/area/type/partner | `404` naming the entity |
| Concurrent same city+type code (race) | server retries; if retries exhausted → `409` |
| Update deactivated facility | `400` "A deactivated facility cannot be updated." |
| Activate under a deactivated branch | `400` "Cannot activate a facility under a deactivated branch." |
| Activate an already active facility | `400` "The facility is already activated." |
| Partner start date not later than current | `400` (BR-FAC-09) |
| FA/DE tries to add/edit a facility | `403` — policy is OrganizationLevel (OQ-33 vs BR-FAC-06) |

## Postconditions
- Facility usable by DataEntry/FacilityAdmin users bound to it; beneficiary codes derive from its code.

## Screens (for frontend)
- Branch → facilities list, facility wizard (picker for area/type/partner + date), partner history timeline, facility type admin.
