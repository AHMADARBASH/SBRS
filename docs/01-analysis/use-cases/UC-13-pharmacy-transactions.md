# UC-13 Pharmacy transactions
> Status: Draft | Last updated: 2026-11-07 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Pharmacy `[PLANNED]` |
| Primary actor | Pharmacy role (name/design `[OPEN]` — OQ-01, OQ-02; ADR-015) |
| Implementation status | `[PLANNED]` — no entities, endpoints or tables |
| Related rules | ADR-015 pharmacy staff must never register beneficiaries; beneficiary search limitations; distinct beneficiary counting (OQ-03) |
| Endpoints | none (planned e.g. `api/pharmacy/transactions`) |

## Goal
Record dispensing of medicines to beneficiaries at a pharmacy facility, linked to the global beneficiary registry without creating beneficiaries.

## Preconditions
Medicine catalogue (UC-12), clinics/doctors (UC-11) `[PLANNED]`, beneficiary found via search (UC-07).

## Main flow (proposed)
1. Pharmacist finds the beneficiary **only via search** (FullCode or first+last name) — the register endpoint must be unreachable for the pharmacy role (ADR-015, enforced by policy + role separation decision OQ-02).
2. Pharmacist records a transaction: beneficiary, medicine(s), quantities, clinic/doctor, date.
3. Transactions are immutable after save (corrections via reversal entries) `[ASSUMED]`.
4. Reports aggregate dispensing volumes and can count **distinct beneficiaries served** (OQ-03).

## Alternative flows
- A1 Person not found at the pharmacy: the pharmacy role cannot register; a DataEntry at a registration facility registers the person, then the pharmacist reuses their FullCode.

## Errors and edge cases
| Situation | Expected behaviour (proposed) |
|---|---|
| Pharmacy role calls `POST api/Beneficiaries/Register` | `403` (built-in once the role exists — policy is DataEntry-only today, so this already holds) |
| Out-of-scope beneficiary reference | `404` |
| Dispensing a retired medicine | `400` |

## Postconditions
- Transaction audit-stamped; beneficiaries untouched (no writes to `beneficiaries`).

## Screens (for frontend)
- Dispensing screen with beneficiary search, medicine line-items, transaction log.

> Open dependency: role name/design (OQ-01, OQ-02) — the entire screen identity depends on it.
