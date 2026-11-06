# UC-12 Manage medicines
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Pharmacy `[PLANNED]` |
| Primary actor | SystemAdmin (global catalogue), OrganizationAdmin (propose/accept) |
| Implementation status | `[PLANNED]` — no entities, endpoints or tables |
| Related rules | (planned) catalogue with approval workflow; separation from beneficiary data |
| Endpoints | none (planned e.g. `api/medicines`) |

## Goal
Maintain a shared catalogue of medicines (and related supplies) that pharmacy transactions (UC-13) can reference, with a controlled add/remove/accept lifecycle.

## Preconditions
Pharmacy module design confirmed (OQ-01/OQ-02).

## Main flow (proposed)
1. SA adds a medicine to the global catalogue: names, form/strength, active flag.
2. OrganizationAdmin proposes a new item or a change for their organization; SA (or an OA-accept step) accepts it.
3. Removal is a soft-delete/retire (BR-GEN-02) so historical transactions keep referencing the item.
4. Pharmacists only ever *select* catalogue items when dispensing (UC-13) — they never manage the catalogue.

## Alternative flows
- A1 Duplicate proposal → conflict error against existing name (following the code-exists patterns).

## Errors and edge cases
| Situation | Expected behaviour (proposed) |
|---|---|
| Edit of a retired medicine | `400` (like other deactivated entities) |
| Catalogue item referenced by transactions | retire instead of remove; reports keep counting it |

## Postconditions
- A stable catalogue identity for every dispensed item.

## Screens (for frontend)
- Medicines catalogue (SA), proposal/acceptance queue (OA), item picker inside the dispensing screen.

> Open dependency: none of this exists in the code; content follows the original module plan (Section 4) and ADR-015.
