# UC-06 Browse geography and manage addresses
> Status: Draft | Last updated: 2026-11-15 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Geography + Addresses |
| Primary actor | all users (browse); SA/OA (address management) |
| Implementation status | `[BUILT]` (geography write endpoints deliberately absent; address deactivate absent — OQ-17) |
| Related rules | BR-GEO-01…06 |
| Endpoints | `GET api/Geography/*`, `GET/POST/PUT api/Addresses/*` |

## Goal
Give every screen consistent, read-only administrative geography (OCHA hierarchy) and a shared address book per area.

## Preconditions
Geography tables are seeded `[OPEN]` (OQ-21) — nothing ships in the repo.

## Main flow
1. Any screen needing an address picker cascades: `Countries` → `Countries/{id}/Cities` → `Cities/{id}/Regions` (or districts) → `Districts/{id}/Areas` — or straight `Cities/{id}/Areas` (BR-GEO-02).
2. Within an area, list addresses `GET api/Addresses/GetByAreaId/{areaId}?includeInactive=`.
3. If the address is new: SA/OA adds it (`areaId`, `arName`, `enName`) → 201; the address is shared system-wide (BR-GEO-03) and is used for working locations, not by facilities.
4. Typo fix: `PUT api/Addresses/Update/{id}` (names only; area immutable).

## Alternative flows
- A1 Mobile team selects a facility's working location for the day (`WorkingAddresses`) `[PLANNED]` (BR-GEO-05).

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| Duplicate (area, name) on add/update | `409` unique violation |
| Unknown areaId | `404` "The selected area does not exist." (register flow message; add returns NotFound) |
| Address deactivation guard | no longer applicable: facilities reference areas directly, not addresses (removed 2026-11-17) |
| `includeInactive` param | honored on `GetByAreaId` `[BUILT]` |

## Postconditions
- A reusable address exists for beneficiary registration (UC-08) and working locations; facilities reference areas directly.

## Screens (for frontend)
- Cascading geography selects (shared component), address-list tab per area, address form.
