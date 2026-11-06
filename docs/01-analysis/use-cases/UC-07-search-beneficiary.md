# UC-07 Search for a beneficiary
> Status: Draft | Last updated: 2026-11-06 | Owner: SBRS backend

| Item | Value |
|---|---|
| Module | Beneficiaries |
| Primary actor | DataEntry, FacilityAdmin, SystemAdmin (`SearchBeneficiaries`) |
| Implementation status | `[BUILT]` |
| Related rules | BR-BEN-02, BR-BEN-03, BR-BEN-10, BR-BEN-11, BR-GEN-05; policies `SearchBeneficiaries` |
| Endpoints | `POST api/Beneficiaries/Search` |

## Goal
Answer "does this person already exist anywhere in the network?" **before** creating a record — search is mandatory-first in the registration flow (UC-08).

## Preconditions
Authenticated DE/FA/SA.

## Main flow
1. User enters either a **FullCode** (`{facility code}-{n}`) or **first name + last name** (≥ 2 normalized characters each).
2. `POST api/Beneficiaries/Search` with `{ fullCode? , firstName?, lastName? }`.
3. Server matches on normalized names (starts-with) across **all facilities**, inactive records excluded, merged duplicates hidden; a merged FullCode resolves to the surviving record `[BUILT]`.
4. Response: at most **20** items + `hasMore` flag `[ASSUMED][BUILT]`.
5. Each hit shows `fullCode`, first/father/last name, birth date (+estimated), gender, and the identity **type code only** (BR-BEN-11 — never the document value).
6. If the person is found and it is the same person → stop; registration must not continue for the same person (see UC-08 A1). Enrollment into an additional facility is `[PLANNED]` (`FacilityBeneficiary`).
7. If not found → proceed to UC-08.

## Alternative flows
- A1 Searching a twin-pattern record: the person shares mother/last/birth date with another → appears as distinct records with their own FullCodes (twin rule keeps them separate, always human-reviewed).
- A2 Search with a full code of a deactivated person → not returned (inactive hidden from search) `[BUILT]`.

## Errors and edge cases
| Situation | System behaviour |
|---|---|
| Neither mode provided / names shorter than 2 normalized chars | `400` "Provide the full code, or both first name and last name (at least 2 characters each)." |
| Only one of first/last name | `400` (both required for the name mode) |
| DE of facility A sees hits from facility B | allowed — this is a cross-facility search by design; only minimal demographics are shown |
| Search audit/rate limiting | `[PLANNED]` (NFR-PRV-04/SEC-10) |

## Postconditions
- Either the existing person is reused, or a register flow starts with the same input echoed into the form.

## Screens (for frontend)
- Universal search box with mode switch (code / names), result table with "register anyway" guard, low-information result tiles in cross-facility mode.
