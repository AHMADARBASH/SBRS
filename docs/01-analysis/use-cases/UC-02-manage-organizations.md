# UC-02 Manage organizations
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Organizations |
| Primary actor | SystemAdmin (add), SystemAdmin + OrganizationAdmin (update own) |
| Implementation status | `[BUILT]` (deactivate endpoint commented out — OQ-15) |
| Related rules | BR-ORG-01, BR-ORG-02, BR-ORG-03, BR-ORG-04, BR-GEN-01…05 |
| Endpoints | `GET api/organizations/GetAll`, `GET api/organizations/GetById/{id}`, `POST api/organizations/Add`, `PUT api/organizations/Update/{id}` |

## Goal
Model the top-level organizations (SARC itself and any other org owning branches) with a unique system-wide code.

## Preconditions
Actor holds the management role for the target organization (SA always, OA only their own).

## Main flow
1. SA opens the organizations list (`GetAll` — OA sees only their own via scoping).
2. SA creates an organization: `arName`, `enName`, `code` → `201` with the DTO.
3. SA/OA edit names or code later (`Update/{id}` → 200). OA's own id is resolved from the claim; passing another organization's id results in **404**.

## Alternative flows
- A1 Re-typing an existing code → handler pre-check + DB unique index; `409` "already exists".

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| Duplicate code (race) | `409` PostgreSQL unique-violation message |
| OA updates a foreign organization | `404` (scoped as not-found) |
| Update a deactivated organization | `400` "A deactivated organization cannot be updated." |
| Double deactivate | `400` "The organization is already deactivated." |
| Deactivate org | endpoint currently commented out (OQ-15) → not possible today |

## Postconditions
- New organization exists with audit stamps; branches can be created beneath it (UC-03).

## Screens (for frontend)
- Organizations list (SA), organization form (add/edit — SA; edit own for OA).
