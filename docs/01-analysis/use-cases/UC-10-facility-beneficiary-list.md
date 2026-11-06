# UC-10 List my facility's beneficiaries
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Beneficiaries |
| Primary actor | FacilityAdmin, DataEntry (own facility); SystemAdmin with `facilityId` param |
| Implementation status | `[BUILT]` |
| Related rules | BR-BEN-10, BR-BEN-14; policy `ViewFacilityBeneficiaries` |
| Endpoints | `GET api/Beneficiaries/GetAll?page={n}&facilityId={guid}`, `GET api/Beneficiaries/{id}/Identities`, `DELETE api/Beneficiaries/{id}` |

## Goal
Show the paginated roster of one facility's beneficiaries as the day-to-day working list.

## Preconditions
Actor bound to a facility (FA/DE); SA must pass `facilityId` explicitly.

## Main flow
1. Client requests page 1; response is `PagedResult<BeneficiaryListItemDto>`: pageSize **30**, sorted newest first (`createdAt` desc), **active records only** `[BUILT]`.
2. Client navigates pages via `NextPage` (`page` up to the validator's cap), using `hasNextPage`.
3. Scope: the caller's facility is implied by the claim — a FA/DE cannot read another facility's roster (they get that facility filtered/404 semantics `[BUILT]`).
4. For one beneficiary, identities open on demand: `GET {id}/Identities` (type names + value, own facility or SA).
5. A wrongly created record is soft-deleted: `DELETE api/Beneficiaries/{id}` → 204 (policy `DeactivateBeneficiaries`: SA/FA/DE).

## Alternative flows
- A1 "Within the facility" means `registration_facility_id` until the `FacilityBeneficiary` enrollment model exists (BR-BEN-14) `[PLANNED]` — after that, roster contents change to enrollments, not registrations.

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| SA without facilityId | `400` |
| FA/DE without a facility claim | `403` |
| Out-of-scope facilityId (SA passes one they can see — SA sees all; others get scoped/404 behaviour) | `404` |
| Identities of a foreign-facility beneficiary | `404` (out-of-scope rule) |

## Postconditions
- The roster is consistent with match decisions: merged records no longer appear; ConfirmedDistinct stays.

## Screens (for frontend)
- Facility roster with pagination, per-row badges for review status, identities drawer, confirm-soft-delete dialog.
